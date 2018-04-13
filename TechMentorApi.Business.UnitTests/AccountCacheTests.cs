namespace TechMentorApi.Business.UnitTests
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountCacheTests
    {
        [Fact]
        public void GetAccountReturnsCachedAccountTest()
        {
            var expected = new Account
            {
                Id = Guid.NewGuid(),
                Provider = Guid.NewGuid().ToString(),
                Subject = Guid.NewGuid().ToString()
            };
            var username = expected.Provider + "|" + expected.Subject;
            var cacheKey = "Account|" + username;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected.Id;

                    return true;
                });

            var sut = new AccountCache(cache, config);

            var actual = sut.GetAccount(username);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAccountReturnsNullWhenCachedAccountNotFoundTest()
        {
            var username = Guid.NewGuid().ToString();
            var cacheKey = "Account|" + username;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(x => false);

            var sut = new AccountCache(cache, config);

            var actual = sut.GetAccount(username);

            actual.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetAccountThrowsExceptionWithInvalidUsernameTest(string username)
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new AccountCache(cache, config);

            Action action = () => sut.GetAccount(username);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void StoreAccountThrowsExceptionWithNullAccountTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new AccountCache(cache, config);

            Action action = () => sut.StoreAccount(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreAccountWritesAccountToCacheTest()
        {
            var expected = Model.Create<Account>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            var cacheKey = "Account|" + expected.Username;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.AccountExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new AccountCache(cache, config);

            sut.StoreAccount(expected);

            cacheEntry.Value.Should().Be(expected.Id);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }
    }
}