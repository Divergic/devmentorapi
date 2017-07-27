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

    public class ProfileStoreTests
    {
        [Fact]
        public async Task GetProfileReturnsNullWhenProfileNotFoundTest()
        {
            var accountId = Guid.NewGuid();

            var sut = new ProfileStore(Config.Storage);

            var actual = await sut.GetProfile(accountId, CancellationToken.None);

            actual.Should().BeNull();
        }

        [Fact]
        public void GetProfileThrowsExceptionWithInvalidParametersTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StoreProfileWritesProfileToStorageTest()
        {
            var newProfile = Model.Create<Profile>();

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(newProfile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetProfile(newProfile.AccountId, CancellationToken.None).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newProfile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task StoreProfileStoresProfileWhenTableNotFoundTest()
        {
            // Retrieve storage Profile from connection-string
            var storageProfile = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageProfile.CreateCloudTableClient();

            var table = client.GetTableReference("Profiles");

            await table.DeleteIfExistsAsync();

            var newProfile = Model.Create<Profile>();

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(newProfile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetProfile(newProfile.AccountId, CancellationToken.None).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newProfile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void StoreProfileThrowsExceptionWithNullProfileTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut.StoreProfile(null, CancellationToken.None).ConfigureAwait(false);

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

            Action action = () => new ProfileStore(config);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigurationTest()
        {
            Action action = () => new ProfileStore(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}