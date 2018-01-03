namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;
    using TechMentorApi.Model;

    public class CacheManager : ICacheManager
    {
        private const string CategoriesCacheKey = "Categories";
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;

        public CacheManager(IMemoryCache cache, ICacheConfig config)
        {
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(config, nameof(config));

            _cache = cache;
            _config = config;
        }

        public ICollection<Category> GetCategories()
        {
            return _cache.Get<ICollection<Category>>(CategoriesCacheKey);
        }

        public Category GetCategory(CategoryGroup group, string name)
        {
            var cacheKey = BuildCategoryCacheKey(group, name);

            return _cache.Get<Category>(cacheKey);
        }

        public ICollection<Guid> GetCategoryLinks(ProfileFilter filter)
        {
            Ensure.Any.IsNotNull(filter, nameof(filter));

            var cacheKey = BuildCategoryLinkCacheKey(filter);

            return _cache.Get<ICollection<Guid>>(cacheKey);
        }

        public void RemoveCategories()
        {
            _cache.Remove(CategoriesCacheKey);
        }

        public void RemoveCategory(Category category)
        {
            var cacheKey = BuildCategoryCacheKey(category.Group, category.Name);

            _cache.Remove(cacheKey);
        }

        public void RemoveCategoryLinks(ProfileFilter filter)
        {
            Ensure.Any.IsNotNull(filter, nameof(filter));

            var cacheKey = BuildCategoryLinkCacheKey(filter);

            _cache.Remove(cacheKey);
        }

        public void StoreCategories(ICollection<Category> categories)
        {
            Ensure.Any.IsNotNull(categories, nameof(categories));

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoriesExpiration
            };

            _cache.Set(CategoriesCacheKey, categories, options);
        }

        public void StoreCategory(Category category)
        {
            Ensure.Any.IsNotNull(category, nameof(category));

            var cacheKey = BuildCategoryCacheKey(category.Group, category.Name);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoriesExpiration
            };

            _cache.Set(cacheKey, category, options);
        }

        public void StoreCategoryLinks(ProfileFilter filter, ICollection<Guid> links)
        {
            Ensure.Any.IsNotNull(filter, nameof(filter));
            Ensure.Any.IsNotNull(links, nameof(links));

            var cacheKey = BuildCategoryLinkCacheKey(filter);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoryLinksExpiration
            };

            _cache.Set(cacheKey, links, options);
        }

        private static string BuildCategoryCacheKey(CategoryGroup group, string name)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Category|" + group + "|" + name;
        }

        private static string BuildCategoryLinkCacheKey(ProfileFilter filter)
        {
            Debug.Assert(filter != null);

            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;
        }
    }
}