namespace TechMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;
    using EnsureThat;

    public class AccountManager : IAccountManager
    {
        private readonly IAccountStore _accountStore;
        private readonly ICacheManager _cache;
        private readonly IProfileStore _profileStore;

        public AccountManager(IAccountStore accountStore, IProfileStore profileStore, ICacheManager cache)
        {
            Ensure.That(accountStore, nameof(accountStore)).IsNotNull();
            Ensure.That(profileStore, nameof(profileStore)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _accountStore = accountStore;
            _profileStore = profileStore;
            _cache = cache;
        }

        public async Task<Account> GetAccount(User user, CancellationToken cancellationToken)
        {
            Ensure.That(user, nameof(user)).IsNotNull();

            var account = _cache.GetAccount(user.Username);

            if (account != null)
            {
                return account;
            }

            // Split the provider and username from the username
            var parts = user.Username.Split(
                new[]
                {
                    '|'
                },
                StringSplitOptions.RemoveEmptyEntries);
            var provider = "Unspecified";
            var username = user.Username;

            if (parts.Length > 1)
            {
                provider = parts[0];
                username = parts[1];
            }

            account = await _accountStore.GetAccount(provider, username, cancellationToken).ConfigureAwait(false);

            if (account == null)
            {
                var profileId = Guid.NewGuid();

                // This account doesn't exist so we will create it here
                var accountTask = CreateAccount(profileId, provider, username, cancellationToken);
                var profileTask = CreateProfile(profileId, user, cancellationToken);

                // Run the tasks together to save time
                await Task.WhenAll(accountTask, profileTask).ConfigureAwait(false);

                var profile = profileTask.Result;

                _cache.StoreProfile(profile);

                account = accountTask.Result;
            }

            _cache.StoreAccount(account);

            return account;
        }

        private async Task<Account> CreateAccount(
            Guid profileId,
            string provider,
            string username,
            CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Id = profileId,
                Provider = provider,
                Username = username
            };

            await _accountStore.RegisterAccount(account, cancellationToken).ConfigureAwait(false);

            return account;
        }

        private async Task<Profile> CreateProfile(Guid profileId, User user, CancellationToken cancellationToken)
        {
            var profile = new Profile
            {
                Id = profileId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            await _profileStore.StoreProfile(profile, cancellationToken).ConfigureAwait(false);

            return profile;
        }
    }
}