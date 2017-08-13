namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class AccountManagerTests
    {
        [Fact]
        public async Task GetAccountCachesAccountReturnedFromStoreTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var expected = Model.Create<Account>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreAccount(expected);
            }
        }

        [Fact]
        public async Task GetAccountCachesCreatedProfileWhenNotFoundInStoreTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received(1).StoreProfile(Arg.Any<Profile>());
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.Id == actual.Id));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.Email == user.Email));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.FirstName == user.FirstName));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.LastName == user.LastName));
            }
        }

        [Fact]
        public async Task GetAccountCachesRegisteredAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreAccount(actual);
            }
        }

        [Fact]
        public async Task GetAccountCreatesProfileWhenNotFoundInStoreTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await profileStore.Received(1).StoreProfile(Arg.Any<Profile>(), tokenSource.Token)
                    .ConfigureAwait(false);
                await profileStore.Received().StoreProfile(Arg.Is<Profile>(x => x.Id == actual.Id), tokenSource.Token)
                    .ConfigureAwait(false);
                await profileStore.Received().StoreProfile(
                    Arg.Is<Profile>(x => x.Email == user.Email),
                    tokenSource.Token).ConfigureAwait(false);
                await profileStore.Received().StoreProfile(
                    Arg.Is<Profile>(x => x.FirstName == user.FirstName),
                    tokenSource.Token).ConfigureAwait(false);
                await profileStore.Received().StoreProfile(
                    Arg.Is<Profile>(x => x.LastName == user.LastName),
                    tokenSource.Token).ConfigureAwait(false);
                await profileStore.Received().StoreProfile(
                    Arg.Is<Profile>(x => x.Status == ProfileStatus.Hidden),
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task GetAccountDefaultsToUnspecifiedProviderWhenNotFoundInUsernameTest()
        {
            var user = Model.Create<User>();
            var expected = Model.Create<Account>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount("Unspecified", user.Username, tokenSource.Token).Returns(expected);

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

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await accountStore.Received(1).RegisterAccount(Arg.Any<Account>(), tokenSource.Token)
                    .ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Id != Guid.Empty), tokenSource.Token).ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Provider == provider), tokenSource.Token)
                    .ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Username == username), tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Id.Should().NotBeEmpty();
                actual.Provider.Should().Be(provider);
                actual.Username.Should().Be(username);
            }
        }

        [Fact]
        public async Task GetAccountRegistersNewAccountWithUnspecifiedProviderWhenNotFoundInStoreTest()
        {
            var user = Model.CreateWith<User>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await accountStore.Received(1).RegisterAccount(Arg.Any<Account>(), tokenSource.Token)
                    .ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Id != Guid.Empty), tokenSource.Token).ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Provider == "Unspecified"), tokenSource.Token)
                    .ConfigureAwait(false);
                await accountStore.Received()
                    .RegisterAccount(Arg.Is<Account>(x => x.Username == user.Username), tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Id.Should().NotBeEmpty();
                actual.Provider.Should().Be("Unspecified");
                actual.Username.Should().Be(user.Username);
            }
        }

        [Fact]
        public async Task GetAccountReturnsAccountByProviderAndUsernameTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var expected = Model.Create<Account>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetAccountReturnsCachedAccountTest()
        {
            var user = Model.Create<User>();
            var expected = Model.Create<Account>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetAccount(user.Username).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void GetAccountThrowsExceptionWithNullUserTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            Func<Task> action = async () => await sut.GetAccount(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullAccountStoreTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new AccountManager(null, profileStore, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();

            Action action = () => new AccountManager(accountStore, profileStore, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new AccountManager(accountStore, null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}