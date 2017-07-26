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
        public void BanAccountThrowsExceptionWhenAccountNotFoundTest()
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut
                .BanAccount(Guid.NewGuid(), DateTimeOffset.UtcNow, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<EntityNotFoundException>();
        }

        [Fact]
        public void BanAccountThrowsExceptionWithEmptyAccountIdTest()
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut
                .BanAccount(Guid.Empty, DateTimeOffset.UtcNow, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task BanAccountUpdatesExistingAccountTest()
        {
            var newAccount = Model.Create<NewAccount>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(newAccount, CancellationToken.None).ConfigureAwait(false);

            var stored = await sut.GetAccount(newAccount.Provider, newAccount.Username, CancellationToken.None)
                .ConfigureAwait(false);

            await sut.BanAccount(stored.Id, bannedAt, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAccount(newAccount.Provider, newAccount.Username, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newAccount, opt => opt.ExcludingMissingMembers());
            actual.BannedAt.Should().Be(bannedAt);
        }

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
            var newAccount = Model.Create<NewAccount>();

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(newAccount, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAccount(newAccount.Provider, newAccount.Username, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newAccount, opt => opt.ExcludingMissingMembers());
            actual.BannedAt.Should().NotHaveValue();
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

            var newAccount = Model.Create<NewAccount>();

            var sut = new AccountStore(Config.Storage);

            await sut.RegisterAccount(newAccount, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAccount(newAccount.Provider, newAccount.Username, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newAccount, opt => opt.ExcludingMissingMembers());
            actual.BannedAt.Should().NotHaveValue();
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