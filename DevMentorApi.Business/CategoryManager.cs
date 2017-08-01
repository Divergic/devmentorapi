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
        private const string CacheKey = "Categories";
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

        public async Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken)
        {
            Ensure.That(newCategory, nameof(newCategory)).IsNotNull();

            var category = new Category
            {
                Group = newCategory.Group,
                LinkCount = 0,
                Name = newCategory.Name,
                Reviewed = true,
                Visible = true
            };

            await _store.StoreCategory(category, cancellationToken).ConfigureAwait(false);

            _cache.Remove(CacheKey);
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
            IEnumerable<Category> categories;

            if (_cache.TryGetValue(CacheKey, out categories))
            {
                return categories;
            }

            var results = await _store.GetAllCategories(cancellationToken).ConfigureAwait(false);

            if (results == null)
            {
                return new List<Category>();
            }

            var storedItems = results.ToList();

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.CategoriesExpiration
            };

            // Cache this account for lookup later
            _cache.Set(CacheKey, storedItems, options);

            return storedItems;
        }
    }
}