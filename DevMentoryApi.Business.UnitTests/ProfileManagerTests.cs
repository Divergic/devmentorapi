namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class ProfileManagerTests
    {
        [Fact]
        public async Task BanProfileCallsStoreWithBanInformationTest()
        {
            var accountId = Guid.NewGuid();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();

            var sut = new ProfileManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.BanProfile(accountId, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await store.Received().BanProfile(accountId, bannedAt, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void BanProfileThrowsExceptionWithEmptyAccountIdTest()
        {
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();

            var sut = new ProfileManager(store, cache, config);

            Func<Task> action = async () => await sut.BanProfile(Guid.Empty, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GetProfileThrowsExceptionWithEmptyAccountIdTest()
        {
            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();

            var sut = new ProfileManager(store, cache, config);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new ProfileManager(profileStore, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<IMemoryCache>();

            Action action = () => new ProfileManager(profileStore, cache, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new ProfileManager(null, cache, config);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}