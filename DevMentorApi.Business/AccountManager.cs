namespace DevMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;

    public class AccountManager : IAccountManager
    {
        private readonly IAccountStore _accountStore;
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;
        private readonly IProfileStore _profileStore;

        public AccountManager(
            IAccountStore accountStore,
            IProfileStore profileStore,
            IMemoryCache cache,
            ICacheConfig config)
        {
            Ensure.That(accountStore, nameof(accountStore)).IsNotNull();
            Ensure.That(profileStore, nameof(profileStore)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _accountStore = accountStore;
            _profileStore = profileStore;
            _cache = cache;
            _config = config;
        }

        public async Task<Account> GetAccount(User user, CancellationToken cancellationToken)
        {
            Ensure.That(user, nameof(user)).IsNotNull();

            Account account;

            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            var cacheKey = "Account|" + user.Username;

            if (_cache.TryGetValue(cacheKey, out account))
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
                var accountId = Guid.NewGuid();

                // This account doesn't exist so we will create it here
                var accountTask = CreateAccount(accountId, provider, username, cancellationToken);
                var profileTask = CreateProfile(accountId, user, cancellationToken);

                // Run the tasks together to save time
                await Task.WhenAll(accountTask, profileTask).ConfigureAwait(false);

                account = accountTask.Result;
            }

            // Cache this account for lookup later
            var cacheEntry = _cache.CreateEntry(cacheKey);

            cacheEntry.SlidingExpiration = _config.AccountExpiration;
            cacheEntry.Value = account;

            return account;
        }

        private async Task<Account> CreateAccount(
            Guid accountId,
            string provider,
            string username,
            CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Id = accountId,
                Provider = provider,
                Username = username
            };

            await _accountStore.RegisterAccount(account, cancellationToken).ConfigureAwait(false);
            
            return account;
        }

        private Task CreateProfile(Guid accountId, User user, CancellationToken cancellationToken)
        {
            var profile = new Profile
            {
                AccountId = accountId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return _profileStore.StoreProfile(profile, cancellationToken);
        }
    }
}