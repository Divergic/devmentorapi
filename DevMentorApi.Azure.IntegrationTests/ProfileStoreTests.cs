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
        public void BanProfileThrowsExceptionWhenProfileNotFoundTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut
                .BanProfile(Guid.NewGuid(), DateTimeOffset.UtcNow, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<EntityNotFoundException>();
        }

        [Fact]
        public void BanProfileThrowsExceptionWithEmptyAccountIdTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut
                .BanProfile(Guid.Empty, DateTimeOffset.UtcNow, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task BanProfileUpdatesExistingProfileTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(profile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.BanProfile(profile.AccountId, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.Excluding(x => x.BannedAt));
            actual.BannedAt.Should().Be(bannedAt);
        }

        [Fact]
        public async Task GetProfileReturnsNullWhenProfileNotFoundTest()
        {
            var accountId = Guid.NewGuid();

            var sut = new ProfileStore(Config.Storage);

            var actual = await sut.GetProfile(accountId, CancellationToken.None);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileReturnsStoredProfileTest()
        {
            var expected = Model.Create<Profile>();

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(expected, CancellationToken.None);

            var actual = await sut.GetProfile(expected.AccountId, CancellationToken.None);

            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void GetProfileThrowsExceptionWithEmptyAccountIdTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StoreProfileCreatesTableAndWritesProfileWhenTableNotFoundTest()
        {
            // Retrieve storage Profile from connection-string
            var storageProfile = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageProfile.CreateCloudTableClient();

            var table = client.GetTableReference("Profiles");

            await table.DeleteIfExistsAsync();

            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(profile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetProfile(profile.AccountId, CancellationToken.None).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void StoreProfileThrowsExceptionWithNullProfileTest()
        {
            var sut = new ProfileStore(Config.Storage);

            Func<Task> action = async () => await sut.StoreProfile(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task StoreProfileWritesProfileToStorageTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(profile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetProfile(profile.AccountId, CancellationToken.None).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task StoreProfileWritesUpdatedProfileToStorageTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);

            var sut = new ProfileStore(Config.Storage);

            await sut.StoreProfile(profile, CancellationToken.None).ConfigureAwait(false);

            profile.FirstName = Guid.NewGuid().ToString();

            await sut.StoreProfile(profile, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetProfile(profile.AccountId, CancellationToken.None).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
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