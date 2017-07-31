namespace DevMentorApi.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;

    public class CategoryManager : ICategoryManager
    {
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;
        private readonly ICategoryStore _store;

        public CategoryManager(ICategoryStore store, IMemoryCache cache, ICacheConfig config)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _store = store;
            _cache = cache;
            _config = config;
        }

        public async Task<IEnumerable<Category>> GetCategories(ReadType readType, CancellationToken cancellationToken)
        {
            var categories = await GetCategoriesInternal(cancellationToken);

            if (readType == ReadType.All)
            {
                return categories;
            }

            return from x in categories
                where x.Visible
                select x;
        }

        private async Task<IEnumerable<Category>> GetCategoriesInternal(CancellationToken cancellationToken)
        {
            const string CacheKey = "Categories";
            IEnumerable<Category> categories;

            if (_cache.TryGetValue(CacheKey, out categories))
            {
                return categories;
            }

            categories = await _store.GetAllCategories(cancellationToken).ConfigureAwait(false);

            if (categories == null)
            {
                return new List<Category>();
            }

            // Cache this account for lookup later
            var cacheEntry = _cache.CreateEntry(CacheKey);

            cacheEntry.SlidingExpiration = _config.CategoriesExpiration;
            cacheEntry.Value = categories;

            return categories;
        }
    }
}