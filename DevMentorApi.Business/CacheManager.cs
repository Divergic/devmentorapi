namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheManager : ICacheManager
    {
        private const string CategoriesCacheKey = "Categories";
        private const string ProfileResultsCacheKey = "ProfileResults";
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

        public ICollection<Guid> GetCategoryLinks(ProfileFilter filter)
        {
            Ensure.That(filter, nameof(filter)).IsNotNull();

            var cacheKey = BuildCategoryLinkCacheKey(filter);

            return _cache.Get<ICollection<Guid>>(cacheKey);
        }

        public Profile GetProfile(Guid id)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            var cacheKey = BuildProfileCacheKey(id);

            return _cache.Get<Profile>(cacheKey);
        }

        public ICollection<ProfileResult> GetProfileResults()
        {
            return _cache.Get<ICollection<ProfileResult>>(ProfileResultsCacheKey);
        }

        public void RemoveCategories()
        {
            _cache.Remove(CategoriesCacheKey);
        }

        public void RemoveCategoryLinks(ProfileFilter filter)
        {
            var cacheKey = BuildCategoryLinkCacheKey(filter);

            _cache.Remove(cacheKey);
        }

        public void StoreAccount(Account account)
        {
            Ensure.That(account, nameof(account)).IsNotNull();

            var cacheKey = BuildAccountCacheKey(account.Username);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.AccountExpiration
            };

            _cache.Set(cacheKey, account, options);
        }

        public void StoreCategories(ICollection<Category> categories)
        {
            Ensure.That(categories, nameof(categories)).IsNotNull();

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoriesExpiration
            };

            _cache.Set(CategoriesCacheKey, categories, options);
        }

        public void StoreCategoryLinks(ProfileFilter filter, ICollection<Guid> links)
        {
            Ensure.That(filter, nameof(filter)).IsNotNull();
            Ensure.That(links, nameof(links)).IsNotNull();

            var cacheKey = BuildCategoryLinkCacheKey(filter);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoryLinksExpiration
            };

            _cache.Set(cacheKey, links, options);
        }

        public void StoreProfile(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var cacheKey = BuildProfileCacheKey(profile.Id);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.ProfileExpiration
            };

            _cache.Set(cacheKey, profile, options);
        }

        public void StoreProfileResults(ICollection<ProfileResult> results)
        {
            Ensure.That(results, nameof(results)).IsNotNull();

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.ProfileResultsExpiration
            };

            _cache.Set(ProfileResultsCacheKey, results, options);
        }

        private static string BuildAccountCacheKey(string username)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Account|" + username;
        }

        private static string BuildCategoryLinkCacheKey(ProfileFilter filter)
        {
            Debug.Assert(filter != null);

            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;
        }

        private static string BuildProfileCacheKey(Guid id)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Profile|" + id;
        }
    }
}