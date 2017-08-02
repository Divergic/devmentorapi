﻿namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class ProfileManagerTests
    {
        [Fact]
        public async Task BanProfileCallsStoreWithBanInformationTest()
        {
            var profileId = Guid.NewGuid();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);
            var cacheKey = "Profile|" + profileId;
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new ProfileManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.BanProfile(profileId, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await store.Received().BanProfile(profileId, bannedAt, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void BanProfileThrowsExceptionWithEmptyIdTest()
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
        public async Task BanProfileUpdatesCacheTest()
        {
            var expected = Model.Create<Profile>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);
            var cacheKey = "Profile|" + expected.Id;
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new ProfileManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.BanProfile(expected.Id, bannedAt, tokenSource.Token).Returns(expected);

                await sut.BanProfile(expected.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(expected);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public async Task GetProfileCachesProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>();
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileExpiration.Returns(cacheExpiry);
            cache.CreateEntry("Profile|" + expected.Id).Returns(cacheEntry);

            var sut = new ProfileManager(profileStore, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(actual);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public async Task GetProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>();
            var cacheKey = "Profile|" + expected.Id;

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new ProfileManager(profileStore, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsNullWhenProfileNotFoundTest()
        {
            var expected = Model.Create<Profile>();

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileManager(profileStore, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().CreateEntry(Arg.Any<object>());
            }
        }

        [Fact]
        public void GetProfileThrowsExceptionWithEmptyIdTest()
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

        [Fact]
        public async Task UpdateProfileStoresProfileAndUpdatesCacheTest()
        {
            var expected = Model.Create<Profile>();
            var cacheKey = "Profile|" + expected.Id;
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new ProfileManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.UpdateProfile(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received().StoreProfile(expected, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(expected);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public void UpdateProfileThrowsExceptionWithNullProfileTest()
        {
            var store = Substitute.For<IProfileStore>();
            var config = Substitute.For<ICacheConfig>();
            var cache = Substitute.For<IMemoryCache>();

            var sut = new ProfileManager(store, cache, config);

            Func<Task> action = async () => await sut.UpdateProfile(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}