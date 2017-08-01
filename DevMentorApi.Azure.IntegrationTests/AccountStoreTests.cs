namespace DevMentorApi.Azure.IntegrationTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class AccountStoreTests
    {
        [Fact]
        public async Task GetAccountReturnsNullWhenAccountNotFoundTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var actual = await sut.GetAccount(provider, username, CancellationToken.None);

            actual.Should().BeNull();
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

            await table.DeleteIfExistsAsync();

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
    }
}