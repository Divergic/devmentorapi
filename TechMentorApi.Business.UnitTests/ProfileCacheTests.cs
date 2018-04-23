namespace TechMentorApi.Business.UnitTests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfileCacheTests
    {
        [Fact]
        public void GetProfileResultsReturnsCachedProfileResultsTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            const string CacheKey = "ProfileResults";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new ProfileCache(cache, config);

            var actual = sut.GetProfileResults();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetProfileResultsReturnsNullWhenCachedProfileResultsNotFoundTest()
        {
            const string CacheKey = "ProfileResults";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(x => false);

            var sut = new ProfileCache(cache, config);

            var actual = sut.GetProfileResults();

            actual.Should().BeNull();
        }

        [Fact]
        public void GetProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>();
            var cacheKey = "Profile|" + expected.Id;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new ProfileCache(cache, config);

            var actual = sut.GetProfile(expected.Id);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetProfileReturnsNullWhenCachedProfileNotFoundTest()
        {
            var id = Guid.NewGuid();
            var cacheKey = "Profile|" + id;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(x => false);

            var sut = new ProfileCache(cache, config);

            var actual = sut.GetProfile(id);

            actual.Should().BeNull();
        }

        [Fact]
        public void GetProfileThrowsExceptionWithInvalidIdTest()
        {
            var id = Guid.Empty;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileCache(cache, config);

            Action action = () => sut.GetProfile(id);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void RemoveProfileDeletesProfileFromCacheTest()
        {
            var expected = Model.Create<Profile>();
            var cacheKey = "Profile|" + expected.Id;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileCache(cache, config);

            sut.RemoveProfile(expected.Id);

            cache.Received().Remove(cacheKey);
        }

        [Fact]
        public void RemoveProfileThrowsExceptionWithEmptyIdTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileCache(cache, config);

            Action action = () => sut.RemoveProfile(Guid.Empty);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void StoreProfileResultsThrowsExceptionWithNullProfileResultsTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileCache(cache, config);

            Action action = () => sut.StoreProfileResults(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreProfileResultsWritesProfileResultsToCacheTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            const string CacheKey = "ProfileResults";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileResultsExpiration.Returns(cacheExpiry);
            cache.CreateEntry(CacheKey).Returns(cacheEntry);

            var sut = new ProfileCache(cache, config);

            sut.StoreProfileResults(expected);

            cacheEntry.Value.Should().Be(expected);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }

        [Fact]
        public void StoreProfileThrowsExceptionWithNullProfileTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new ProfileCache(cache, config);

            Action action = () => sut.StoreProfile(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreProfileWritesProfileToCacheTest()
        {
            var expected = Model.Create<Profile>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            var cacheKey = "Profile|" + expected.Id;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.ProfileExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new ProfileCache(cache, config);

            sut.StoreProfile(expected);

            cacheEntry.Value.Should().Be(expected);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new ProfileCache(null, config);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigTest()
        {
            var cache = Substitute.For<IMemoryCache>();

            Action action = () => new ProfileCache(cache, null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}