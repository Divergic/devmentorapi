using EnsureThat;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Azure;
using TechMentorApi.Management;
using TechMentorApi.Model;

namespace TechMentorApi.Business.Commands
{
    public class AccountCommand : IAccountCommand
    {
        private readonly IAccountStore _accountStore;
        private readonly IAccountCache _cache;
        private readonly IPhotoCommand _photoCommand;
        private readonly IProfileCommand _profileCommand;
        private readonly IUserStore _userStore;

        public AccountCommand(IPhotoCommand photoCommand, IProfileCommand profileCommand, IUserStore userStore, IAccountStore accountStore, IAccountCache cache)
        {
            Ensure.Any.IsNotNull(photoCommand, nameof(photoCommand));
            Ensure.Any.IsNotNull(profileCommand, nameof(profileCommand));
            Ensure.Any.IsNotNull(userStore, nameof(userStore));
            Ensure.Any.IsNotNull(accountStore, nameof(accountStore));
            Ensure.Any.IsNotNull(cache, nameof(cache));

            _photoCommand = photoCommand;
            _profileCommand = profileCommand;
            _userStore = userStore;
            _accountStore = accountStore;
            _cache = cache;
        }

        public async Task DeleteAccount(string username, Guid profileId, CancellationToken cancellationToken)
        {
            Ensure.String.IsNotNullOrWhiteSpace(username, nameof(username));
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));

            await _photoCommand.DeletePhotos(profileId, cancellationToken).ConfigureAwait(false);
            await _profileCommand.DeleteProfile(profileId, cancellationToken).ConfigureAwait(false);

            var account = new Account(username);

            await _accountStore.DeleteAccount(account.Provider, account.Subject, cancellationToken).ConfigureAwait(false);
            await _userStore.DeleteUser(username, cancellationToken).ConfigureAwait(false);
            _cache.RemoveAccount(username);
        }
    }
}