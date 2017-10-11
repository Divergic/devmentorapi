namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountStoreTests
    {
        [Fact]
        public async Task GetAccountReturnsAccountWhenConflictFoundCreatingAccountOnAsynchronousRequestsTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var firstTask = sut.GetAccount(provider, username, CancellationToken.None);
            var secondTask = sut.GetAccount(provider, username, CancellationToken.None);
            var thirdTask = sut.GetAccount(provider, username, CancellationToken.None);
            var tasks = new List<Task<AccountResult>> {firstTask, secondTask, thirdTask};

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var expected = firstTask.Result.Id;

            var actual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            actual.Id.Should().Be(expected);
            tasks.Count(x => x.Result.IsNewAccount).Should().Be(1);
            tasks.Count(x => x.Result.IsNewAccount == false).Should().Be(2);
        }

        [Fact]
        public async Task GetAccountReturnsExistingAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var firstActual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            firstActual.IsNewAccount.Should().BeTrue();

            var secondActual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            secondActual.ShouldBeEquivalentTo(firstActual, opt => opt.Excluding(x => x.IsNewAccount));
            secondActual.IsNewAccount.Should().BeFalse();
        }

        [Fact]
        public async Task
            GetAccountReturnsFirstRegisteredIdStoredWhenConflictFoundCreatingAccountOnAsynchronousRequestsTest()
        {
            var expectedId = Guid.NewGuid();
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new ConflictAccountStoreWrapper(Config.Storage, expectedId);

            var actual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            actual.Id.Should().Be(expectedId);

            // In this test we simulated a conflict by writing the entity before the store tried to create it again
            // This means that the result returned will be the originally stored entity such that it is not considered a new entity
            actual.IsNewAccount.Should().BeFalse();
        }

        [Fact]
        public async Task GetAccountReturnsNewAccountWhenNotFoundTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var actual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty();
            actual.Provider.Should().Be(provider);
            actual.Username.Should().Be(username);
            actual.IsNewAccount.Should().BeTrue();
        }

        [Fact]
        public async Task GetAccountReturnsNewAccountWhenTableNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("Accounts");

            await table.DeleteIfExistsAsync().ConfigureAwait(false);

            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var actual = await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty();
            actual.Provider.Should().Be(provider);
            actual.Username.Should().Be(username);
            actual.IsNewAccount.Should().BeTrue();
        }

        [Fact]
        public void GetAccountThrowsExceptionWhenFailingToCreateAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new FailOnConflictAccountStoreWrapper(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetAccount(provider, username, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<StorageException>();
        }

        [Theory]
        [InlineData(null, "stuff")]
        [InlineData("", "stuff")]
        [InlineData(" ", "stuff")]
        [InlineData("stuff", null)]
        [InlineData("stuff", "")]
        [InlineData("stuff", " ")]
        public void GetAccountThrowsExceptionWithInvalidParametersTest(string provider, string username)
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut.GetAccount(provider, username, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task RegisterAccountStoresNewAccountTest()
        {
            var account = Model.Create<Account>();

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(account, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAccount(account.Provider, account.Username, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(account, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task RegisterAccountStoresNewAccountWhenTableNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("Accounts");

            await table.DeleteIfExistsAsync().ConfigureAwait(false);

            var account = Model.Create<Account>();

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(account, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAccount(account.Provider, account.Username, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(account, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task RegisterAccountThrowsExceptionWhenAccountAlreadyExistsTest()
        {
            var account = Model.Create<Account>();

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(account, CancellationToken.None).ConfigureAwait(false);

            Func<Task> action = async () => await sut.RegisterAccount(account, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<StorageException>();
        }

        [Fact]
        public void RegisterAccountThrowsExceptionWithNullAccountTest()
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut.RegisterAccount(null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ThrowsExceptionWhenCreatedWithInvalidConnectionStringTest(string connectionString)
        {
            var config = Substitute.For<IStorageConfiguration>();

            config.ConnectionString.Returns(connectionString);

            Action action = () => new AccountStore(config);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigurationTest()
        {
            Action action = () => new AccountStore(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        private class ConflictAccountStoreWrapper : AccountStore
        {
            private readonly Guid _expectedId;

            public ConflictAccountStoreWrapper(IStorageConfiguration configuration, Guid expectedId)
                : base(configuration)
            {
                _expectedId = expectedId;
            }

            public override async Task RegisterAccount(Account account, CancellationToken cancellationToken)
            {
                var existingAccount = new Account
                {
                    Id = _expectedId,
                    Provider = account.Provider,
                    Username = account.Username
                };
                var adapter = new AccountAdapter(existingAccount);

                // Ensure the entity exists to test the conflict logic
                await InsertEntity("Accounts", adapter, cancellationToken).ConfigureAwait(false);

                await base.RegisterAccount(account, cancellationToken).ConfigureAwait(false);
            }
        }

        private class FailOnConflictAccountStoreWrapper : AccountStore
        {
            public FailOnConflictAccountStoreWrapper(IStorageConfiguration configuration)
                : base(configuration)
            {
            }

            public override async Task RegisterAccount(Account account, CancellationToken cancellationToken)
            {
                var table = GetTable("Accounts");

                // Force a failure scenario that isn't
                await table.DeleteAsync().ConfigureAwait(false);

                var adapter = new AccountAdapter(account);
                var operation = TableOperation.Insert(adapter);

                // This will fail to insert because the table does not exist
                await table.ExecuteAsync(operation).ConfigureAwait(false);
            }
        }
    }
}