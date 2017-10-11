namespace TechMentorApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Azure;
    using TechMentorApi.Business;
    using TechMentorApi.Model;
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
            var expected = Model.Create<AccountResult>().Set(x => x.IsNewAccount = false);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreAccount(expected);
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
                await profileStore.DidNotReceive().StoreProfile(Arg.Any<Profile>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task GetAccountCachesCreatedProfileWhenNewAccountCreatedTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var account = Model.Create<AccountResult>().Set(x => x.IsNewAccount = true);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received(1).StoreProfile(Arg.Any<Profile>());
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.Id == actual.Id));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.Email == user.Email));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.FirstName == user.FirstName));
                cache.Received().StoreProfile(Arg.Is<Profile>(x => x.LastName == user.LastName));
            }
        }

        [Fact]
        public async Task GetAccountCachesNewAccountTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var account = Model.Create<AccountResult>().Set(x => x.IsNewAccount = true);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreAccount(actual);
            }
        }

        [Fact]
        public async Task GetAccountCreatesProfileWhenNewAccountCreatedTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var account = Model.Create<AccountResult>().Set(x => x.IsNewAccount = true);

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new AccountManager(accountStore, profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

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
            var expected = Model.Create<AccountResult>();

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
        public async Task GetAccountReturnsAccountByProviderAndUsernameTest()
        {
            var provider = Guid.NewGuid().ToString();
            var username = Guid.NewGuid().ToString();
            var user = Model.CreateWith<User>(provider + "|" + username);
            var expected = Model.Create<AccountResult>();

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