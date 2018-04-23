namespace TechMentorApi.Business
{
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using TechMentorApi.Model;

    public class AccountCache : IAccountCache
    {
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;

        public AccountCache(IMemoryCache cache, ICacheConfig config)
        {
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(config, nameof(config));

            _cache = cache;
            _config = config;
        }

        public Account GetAccount(string username)
        {
            Ensure.String.IsNotNullOrWhiteSpace(username, nameof(username));

            var cacheKey = BuildAccountCacheKey(username);

            var id = _cache.Get<Guid>(cacheKey);

            if (id == Guid.Empty)
            {
                return null;
            }

            var account = new Account(username)
            {
                Id = id
            };

            return account;
        }

        public void RemoveAccount(string username)
        {
            Ensure.String.IsNotNullOrWhiteSpace(username, nameof(username));

            var cacheKey = BuildAccountCacheKey(username);

            _cache.Remove(cacheKey);
        }

        public void StoreAccount(Account account)
        {
            Ensure.Any.IsNotNull(account, nameof(account));

            var cacheKey = BuildAccountCacheKey(account.Username);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.AccountExpiration
            };

            _cache.Set(cacheKey, account.Id, options);
        }

        private static string BuildAccountCacheKey(string username)
        {
            // The cache key has a prefix to partition this type of object just in case there is a
            // key collision with another object type
            return "Account|" + username;
        }
    }
}