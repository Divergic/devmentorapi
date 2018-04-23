namespace TechMentorApi.Business.UnitTests.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountQueryTests
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                accountCache.Received().StoreAccount(expected);
                profileCache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
                await profileStore.DidNotReceive().StoreProfile(Arg.Any<Profile>(), Arg.Any<CancellationToken>())
                    .ConfigureAwait(false);
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                profileCache.Received(1).StoreProfile(Arg.Any<Profile>());
                profileCache.Received().StoreProfile(Arg.Is<Profile>(x => x.Id == actual.Id));
                profileCache.Received().StoreProfile(Arg.Is<Profile>(x => x.Email == user.Email));
                profileCache.Received().StoreProfile(Arg.Is<Profile>(x => x.FirstName == user.FirstName));
                profileCache.Received().StoreProfile(Arg.Is<Profile>(x => x.LastName == user.LastName));
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                accountCache.Received().StoreAccount(actual);
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(account);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                await profileStore.Received(1).StoreProfile(Arg.Any<Profile>(), tokenSource.Token)
                    .ConfigureAwait(false);
                await profileStore.Received().StoreProfile(Arg.Is<Profile>(x => x.Id == actual.Id), tokenSource.Token)
                    .ConfigureAwait(false);
                await profileStore.Received()
                    .StoreProfile(Arg.Is<Profile>(x => x.Email == user.Email), tokenSource.Token).ConfigureAwait(false);
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount("Unspecified", user.Username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
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
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.GetAccount(provider, username, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetAccountReturnsCachedAccountTest()
        {
            var user = Model.Create<User>();
            var expected = Model.Create<Account>();

            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountCache.GetAccount(user.Username).Returns(expected);

                var actual = await sut.GetAccount(user, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void GetAccountThrowsExceptionWithNullUserTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            var sut = new AccountQuery(accountStore, profileStore, accountCache, profileCache);

            Func<Task> action = async () => await sut.GetAccount(null, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullAccountCacheTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var profileCache = Substitute.For<IProfileCache>();

            Action action = () => new AccountQuery(accountStore, profileStore, null, profileCache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullAccountStoreTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            Action action = () => new AccountQuery(null, profileStore, accountCache, profileCache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileCacheTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var accountCache = Substitute.For<IAccountCache>();

            Action action = () => new AccountQuery(accountStore, profileStore, accountCache, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var accountStore = Substitute.For<IAccountStore>();
            var accountCache = Substitute.For<IAccountCache>();
            var profileCache = Substitute.For<IProfileCache>();

            Action action = () => new AccountQuery(accountStore, null, accountCache, profileCache);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}