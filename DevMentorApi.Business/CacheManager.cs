namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheManager : ICacheManager
    {
        private const string CategoriesCacheKey = "Categories";
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;

        public CacheManager(IMemoryCache cache, ICacheConfig config)
        {
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _cache = cache;
            _config = config;
        }

        public Account GetAccount(string username)
        {
            Ensure.That(username, nameof(username)).IsNotNullOrWhiteSpace();

            var cacheKey = BuildAccountCacheKey(username);

            return _cache.Get<Account>(cacheKey);
        }

        public ICollection<Category> GetCategories()
        {
            return _cache.Get<ICollection<Category>>(CategoriesCacheKey);
        }

        public Profile GetProfile(Guid id)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            var cacheKey = BuildProfileCacheKey(id);

            return _cache.Get<Profile>(cacheKey);
        }

        public void RemoveCategories()
        {
            _cache.Remove(CategoriesCacheKey);
        }

        public void StoreAccount(Account account)
        {
            Ensure.That(account, nameof(account)).IsNotNull();

            var cacheKey = BuildAccountCacheKey(account.Username);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.AccountExpiration
            };

            // Cache this account for lookup later
            _cache.Set(cacheKey, account, options);
        }

        public void StoreCategories(ICollection<Category> categories)
        {
            Ensure.That(categories, nameof(categories)).IsNotNull();

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoriesExpiration
            };

            // Cache this account for lookup later
            _cache.Set(CategoriesCacheKey, categories, options);
        }

        public void StoreProfile(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var cacheKey = BuildProfileCacheKey(profile.Id);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.ProfileExpiration
            };

            // Cache this profile for lookup later
            _cache.Set(cacheKey, profile, options);
        }

        private static string BuildAccountCacheKey(string username)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Account|" + username;
        }

        private static string BuildProfileCacheKey(Guid id)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Profile|" + id;
        }
    }
}