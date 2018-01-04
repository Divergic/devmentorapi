namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class AccountQuery : IAccountQuery
    {
        private readonly IAccountStore _accountStore;
        private readonly IAccountCache _accountCache;
        private readonly IProfileStore _profileStore;
        private readonly IProfileCache _profileCache;

        public AccountQuery(IAccountStore accountStore, IProfileStore profileStore, IAccountCache accountCache, IProfileCache profileCache)
        {
            Ensure.Any.IsNotNull(accountStore, nameof(accountStore));
            Ensure.Any.IsNotNull(profileStore, nameof(profileStore));
            Ensure.Any.IsNotNull(accountCache, nameof(accountCache));
            Ensure.Any.IsNotNull(profileCache, nameof(profileCache));

            _accountStore = accountStore;
            _profileStore = profileStore;
            _accountCache = accountCache;
            _profileCache = profileCache;
        }

        public async Task<Account> GetAccount(User user, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(user, nameof(user));

            var cachedAccount = _accountCache.GetAccount(user.Username);

            if (cachedAccount != null)
            {
                return cachedAccount;
            }

            var parsedAccount = new Account(user.Username);
            
            var account = await _accountStore.GetAccount(parsedAccount.Provider, parsedAccount.Subject, cancellationToken).ConfigureAwait(false);

            if (account.IsNewAccount)
            {
                // This account has just been created
                var profile = await CreateProfile(account.Id, user, cancellationToken).ConfigureAwait(false);

                _profileCache.StoreProfile(profile);
            }

            _accountCache.StoreAccount(account);

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