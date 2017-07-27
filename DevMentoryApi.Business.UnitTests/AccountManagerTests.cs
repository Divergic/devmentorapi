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
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class AccountManagerTests
    {
        [Fact]
        public async Task BanAccountCallsStoreWithBanInformationTest()
        {
            var accountId = Guid.NewGuid();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.BanAccount(accountId, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await store.Received().BanAccount(accountId, bannedAt, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void BanAccountThrowsExceptionWithEmptyAccountIdTest()
        {
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            Func<Task> action = async () => await sut.BanAccount(Guid.Empty, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task GetAccountCachesAccountReturnedFromStoreTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var expected = Model.Create<Account>();
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.AccountCacheTtl.Returns(cacheExpiry);
            cache.CreateEntry("Account|" + user.Username).Returns(cacheEntry);

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(actual);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public async Task GetAccountCachesRegisteredAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.AccountCacheTtl.Returns(cacheExpiry);
            cache.CreateEntry("Account|" + user.Username).Returns(cacheEntry);

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(actual);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public async Task GetAccountDefaultsToUnspecifiedProviderWhenNotFoundInUsernameTest()
        {
            var user = Model.Create<User>();
            var expected = Model.Create<Account>();

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAccount("Unspecified", user.Username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetAccountRegistersNewAccountWhenNotFoundInStoreTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await store.Received(1).RegisterAccount(Arg.Any<NewAccount>(), tokenSource.Token).ConfigureAwait(false);
                await store.Received().RegisterAccount(Arg.Is<NewAccount>(x => x.Id != Guid.Empty), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().RegisterAccount(
                    Arg.Is<NewAccount>(x => x.Provider == provider),
                    tokenSource.Token).ConfigureAwait(false);
                await store.Received().RegisterAccount(
                    Arg.Is<NewAccount>(x => x.Username == username),
                    tokenSource.Token).ConfigureAwait(false);

                actual.Id.Should().NotBeEmpty();
                actual.Provider.Should().Be(provider);
                actual.Username.Should().Be(username);
                actual.BannedAt.Should().NotHaveValue();
            }
        }

        [Fact]
        public async Task GetAccountRegistersNewAccountWithUnspecifiedProviderWhenNotFoundInStoreTest()
        {
            var user = Model.CreateWith<User>();

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await store.Received(1).RegisterAccount(Arg.Any<NewAccount>(), tokenSource.Token).ConfigureAwait(false);
                await store.Received().RegisterAccount(Arg.Is<NewAccount>(x => x.Id != Guid.Empty), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().RegisterAccount(
                    Arg.Is<NewAccount>(x => x.Provider == "Unspecified"),
                    tokenSource.Token).ConfigureAwait(false);
                await store.Received().RegisterAccount(
                    Arg.Is<NewAccount>(x => x.Username == user.Username),
                    tokenSource.Token).ConfigureAwait(false);

                actual.Id.Should().NotBeEmpty();
                actual.Provider.Should().Be("Unspecified");
                actual.Username.Should().Be(user.Username);
                actual.BannedAt.Should().NotHaveValue();
            }
        }

        [Fact]
        public async Task GetAccountReturnsAccountByProviderAndUsernameTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var expected = Model.Create<Account>();

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetAccountReturnsCachedAccountTest()
        {
            var user = Model.Create<User>();
            var expected = Model.Create<Account>();
            var cacheKey = "Account|" + user.Username;

            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new AccountManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void GetAccountThrowsExceptionWithNullUserTest()
        {
            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            var sut = new AccountManager(store, cache, config);

            Func<Task> action = async () => await sut.GetAccount(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<IAccountStore>();
            var config = Substitute.For<IAuthenticationConfig>();

            Action action = () => new AccountManager(store, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigTest()
        {
            var store = Substitute.For<IAccountStore>();
            var cache = Substitute.For<IMemoryCache>();

            Action action = () => new AccountManager(store, cache, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullStoreTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<IAuthenticationConfig>();

            Action action = () => new AccountManager(null, cache, config);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}