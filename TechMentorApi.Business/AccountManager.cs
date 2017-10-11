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

            var cachedAccount = _cache.GetAccount(user.Username);

            if (cachedAccount != null)
            {
                return cachedAccount;
            }

            var parsedAccount = new Account(user.Username);
            
            var account = await _accountStore.GetAccount(parsedAccount.Provider, parsedAccount.Username, cancellationToken).ConfigureAwait(false);

            if (account.IsNewAccount)
            {
                // This account has just been created
                var profile = await CreateProfile(account.Id, user, cancellationToken).ConfigureAwait(false);

                _cache.StoreProfile(profile);
            }

            _cache.StoreAccount(account);

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