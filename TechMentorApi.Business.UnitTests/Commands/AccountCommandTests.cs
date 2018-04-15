using FluentAssertions;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Azure;
using TechMentorApi.Business.Commands;
using TechMentorApi.Management;
using TechMentorApi.Model;
using Xunit;

namespace TechMentorApi.Business.UnitTests.Commands
{
    public class AccountCommandTests
    {
        [Fact]
        public async Task DeleteAccountDoesNotContinueWhenDeleteingAccountThrowsExceptionTest()
        {
            var username = Guid.NewGuid().ToString();
            var profileId = Guid.NewGuid();
            var account = new Account(username);

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            using (var tokenSource = new CancellationTokenSource())
            {
                accountStore.DeleteAccount(account.Provider, account.Subject, tokenSource.Token).Returns(x => throw new InvalidOperationException());

                try
                {
                    await sut.DeleteAccount(username, profileId, tokenSource.Token).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }

                await photoCommand.Received().DeletePhotos(profileId, tokenSource.Token).ConfigureAwait(false);
                await profileCommand.Received().DeleteProfile(profileId, tokenSource.Token).ConfigureAwait(false);
                await accountStore.Received().DeleteAccount(account.Provider, account.Subject, tokenSource.Token).ConfigureAwait(false);
                await userCommand.DidNotReceive().DeleteUser(Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteAccountDoesNotContinueWhenDeleteingPhotosThrowsExceptionTest()
        {
            var username = Guid.NewGuid().ToString();
            var profileId = Guid.NewGuid();
            var account = new Account(username);

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            using (var tokenSource = new CancellationTokenSource())
            {
                photoCommand.DeletePhotos(profileId, tokenSource.Token).Returns(x => throw new InvalidOperationException());

                try
                {
                    await sut.DeleteAccount(username, profileId, tokenSource.Token).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }

                await photoCommand.Received().DeletePhotos(profileId, tokenSource.Token).ConfigureAwait(false);
                await profileCommand.DidNotReceive().DeleteProfile(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
                await accountStore.DidNotReceive().DeleteAccount(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
                await userCommand.DidNotReceive().DeleteUser(Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteAccountDoesNotContinueWhenDeleteingProfileThrowsExceptionTest()
        {
            var username = Guid.NewGuid().ToString();
            var profileId = Guid.NewGuid();
            var account = new Account(username);

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileCommand.DeleteProfile(profileId, tokenSource.Token).Returns(x => throw new InvalidOperationException());

                try
                {
                    await sut.DeleteAccount(username, profileId, tokenSource.Token).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }

                await photoCommand.Received().DeletePhotos(profileId, tokenSource.Token).ConfigureAwait(false);
                await profileCommand.Received().DeleteProfile(profileId, tokenSource.Token).ConfigureAwait(false);
                await accountStore.DidNotReceive().DeleteAccount(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
                await userCommand.DidNotReceive().DeleteUser(Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteAccountRemovesAllProfileAndAccountInformationTest()
        {
            var username = Guid.NewGuid().ToString();
            var profileId = Guid.NewGuid();
            var account = new Account(username);

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.DeleteAccount(username, profileId, tokenSource.Token).ConfigureAwait(false);

                await photoCommand.Received().DeletePhotos(profileId, tokenSource.Token).ConfigureAwait(false);
                await profileCommand.Received().DeleteProfile(profileId, tokenSource.Token).ConfigureAwait(false);
                await userCommand.Received().DeleteUser(username, tokenSource.Token).ConfigureAwait(false);
                await accountStore.Received().DeleteAccount(account.Provider, account.Subject, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void DeleteAccountThrowsExceptionWithInvalidProfileIdTest()
        {
            var username = Guid.NewGuid().ToString();
            var profileId = Guid.Empty;

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            Func<Task> action = async () => await sut.DeleteAccount(username, profileId, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void DeleteAccountThrowsExceptionWithInvalidUsernameTest(string username)
        {
            var profileId = Guid.NewGuid();

            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            var sut = new AccountCommand(photoCommand, profileCommand, userCommand, accountStore);

            Func<Task> action = async () => await sut.DeleteAccount(username, profileId, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullAccountStoreTest()
        {
            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();

            Action action = () => new AccountCommand(photoCommand, profileCommand, userCommand, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullPhotoCommandTest()
        {
            var profileCommand = Substitute.For<IProfileCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            Action action = () => new AccountCommand(null, profileCommand, userCommand, accountStore);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullProfileCommandTest()
        {
            var photoCommand = Substitute.For<IPhotoCommand>();
            var userCommand = Substitute.For<IUserStore>();
            var accountStore = Substitute.For<IAccountStore>();

            Action action = () => new AccountCommand(photoCommand, null, userCommand, accountStore);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullUserCommandTest()
        {
            var photoCommand = Substitute.For<IPhotoCommand>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountStore = Substitute.For<IAccountStore>();

            Action action = () => new AccountCommand(photoCommand, profileCommand, null, accountStore);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}