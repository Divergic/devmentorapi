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
        private readonly IMemoryCache _cache;
        private readonly IAuthenticationConfig _config;
        private readonly IAccountStore _store;

        public AccountManager(IAccountStore store, IMemoryCache cache, IAuthenticationConfig config)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _store = store;
            _cache = cache;
            _config = config;
        }

        public Task BanAccount(Guid accountId, DateTimeOffset bannedAt, CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();

            // There will be no update the account cache here
            // Caching the accounts is for authentication lookups and not about whether the profile is visible to the public
            // Worst case, the user will continue to have access to modify their profile for a small sliding cache expiry window

            // TODO: Update the cache of profiles to remove this account so that it does not appear in the public directory

            return _store.BanAccount(accountId, bannedAt, cancellationToken);
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

            account = await _store.GetAccount(provider, username, cancellationToken).ConfigureAwait(false);

            if (account == null)
            {
                // This account doesn't exist so we will create it here
                account = await CreateAccount(provider, username, cancellationToken);

                // TODO: Create a new profile with the account information
            }

            // Cache this account for lookup later
            var cacheEntry = _cache.CreateEntry(cacheKey);

            cacheEntry.SlidingExpiration = _config.AccountCacheTtl;
            cacheEntry.Value = account;

            return account;
        }

        private async Task<Account> CreateAccount(
            string provider,
            string username,
            CancellationToken cancellationToken)
        {
            var newAccount = new NewAccount
            {
                Id = Guid.NewGuid(),
                Provider = provider,
                Username = username
            };

            await _store.RegisterAccount(newAccount, cancellationToken).ConfigureAwait(false);

            var account = new Account(newAccount);

            return account;
        }
    }
}