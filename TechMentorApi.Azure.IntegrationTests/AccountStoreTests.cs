﻿namespace TechMentorApi.Azure.IntegrationTests
{
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountStoreTests
    {
        [Fact]
        public async Task DeleteAccountIgnoresWhenAccountNotFoundTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            await sut.DeleteAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteAccountRemovesAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            await sut.DeleteAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudTableClient();
            var operation = TableOperation.Retrieve(provider, subject);

            var table = client.GetTableReference("Accounts");

            var result = await table.ExecuteAsync(operation).ConfigureAwait(false);

            result.HttpStatusCode.Should().Be(404);
        }

        [Theory]
        [InlineData(null, "stuff")]
        [InlineData("", "stuff")]
        [InlineData(" ", "stuff")]
        [InlineData("stuff", null)]
        [InlineData("stuff", "")]
        [InlineData("stuff", " ")]
        public void DeleteAccountThrowsExceptionWithInvalidParametersTest(string provider, string subject)
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut.DeleteAccount(provider, subject, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task GetAccountReturnsAccountWhenConflictFoundCreatingAccountOnAsynchronousRequestsTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var firstTask = sut.GetAccount(provider, subject, CancellationToken.None);
            var secondTask = sut.GetAccount(provider, subject, CancellationToken.None);
            var thirdTask = sut.GetAccount(provider, subject, CancellationToken.None);
            var tasks = new List<Task<AccountResult>> { firstTask, secondTask, thirdTask };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var expected = firstTask.Result.Id;

            var actual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            actual.Id.Should().Be(expected);
            tasks.Count(x => x.Result.IsNewAccount).Should().Be(1);
            tasks.Count(x => x.Result.IsNewAccount == false).Should().Be(2);
        }

        [Fact]
        public async Task GetAccountReturnsExistingAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var firstActual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            firstActual.IsNewAccount.Should().BeTrue();

            var secondActual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            secondActual.Should().BeEquivalentTo(firstActual, opt => opt.Excluding(x => x.IsNewAccount));
            secondActual.IsNewAccount.Should().BeFalse();
        }

        [Fact]
        public async Task
            GetAccountReturnsFirstRegisteredIdStoredWhenConflictFoundCreatingAccountOnAsynchronousRequestsTest()
        {
            var expectedId = Guid.NewGuid();
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new ConflictAccountStoreWrapper(Config.Storage, expectedId);

            var actual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            actual.Id.Should().Be(expectedId);

            // In this test we simulated a conflict by writing the entity before the store tried to
            // create it again This means that the result returned will be the originally stored
            // entity such that it is not considered a new entity
            actual.IsNewAccount.Should().BeFalse();
        }

        [Fact]
        public async Task GetAccountReturnsNewAccountWhenNotFoundTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var actual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty();
            actual.Provider.Should().Be(provider);
            actual.Subject.Should().Be(subject);
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
            var subject = Guid.NewGuid().ToString();

            var sut = new AccountStore(Config.Storage);

            var actual = await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty();
            actual.Provider.Should().Be(provider);
            actual.Subject.Should().Be(subject);
            actual.IsNewAccount.Should().BeTrue();
        }

        [Fact]
        public void GetAccountThrowsExceptionWhenFailingToCreateAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var subject = Guid.NewGuid().ToString();

            var sut = new FailOnConflictAccountStoreWrapper(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetAccount(provider, subject, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<StorageException>();
        }

        [Theory]
        [InlineData(null, "stuff")]
        [InlineData("", "stuff")]
        [InlineData(" ", "stuff")]
        [InlineData("stuff", null)]
        [InlineData("stuff", "")]
        [InlineData("stuff", " ")]
        public void GetAccountThrowsExceptionWithInvalidParametersTest(string provider, string subject)
        {
            var sut = new AccountStore(Config.Storage);

            Func<Task> action = async () => await sut.GetAccount(provider, subject, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
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

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigurationTest()
        {
            Action action = () => new AccountStore(null);

            action.Should().Throw<ArgumentNullException>();
        }

        private class ConflictAccountStoreWrapper : AccountStore
        {
            private readonly Guid _expectedId;

            public ConflictAccountStoreWrapper(IStorageConfiguration configuration, Guid expectedId)
                : base(configuration)
            {
                _expectedId = expectedId;
            }

            protected override async Task RegisterAccount(Account account, CancellationToken cancellationToken)
            {
                var existingAccount = new Account
                {
                    Id = _expectedId,
                    Provider = account.Provider,
                    Subject = account.Subject
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

            protected override async Task RegisterAccount(Account account, CancellationToken cancellationToken)
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