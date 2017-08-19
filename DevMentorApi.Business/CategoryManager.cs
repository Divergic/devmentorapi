namespace DevMentorApi.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;

    public class CategoryManager : ICategoryManager
    {
        private readonly ICacheManager _cache;
        private readonly ICategoryStore _store;

        public CategoryManager(ICategoryStore store, ICacheManager cache)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _store = store;
            _cache = cache;
        }

        public async Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken)
        {
            Ensure.That(newCategory, nameof(newCategory)).IsNotNull();

            var existingCategory = await _store.GetCategory(newCategory.Group, newCategory.Name, cancellationToken).ConfigureAwait(false);

            var linkCount = 0;

            if (existingCategory != null)
            {
                linkCount = existingCategory.LinkCount;
            }

            var category = new Category
            {
                Group = newCategory.Group,
                LinkCount = linkCount,
                Name = newCategory.Name,
                Reviewed = true,
                Visible = true
            };

            await _store.StoreCategory(category, cancellationToken).ConfigureAwait(false);

            _cache.RemoveCategories();
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
            var categories = _cache.GetCategories();

            if (categories != null)
            {
                return categories;
            }

            var results = await _store.GetAllCategories(cancellationToken).ConfigureAwait(false);

            if (results == null)
            {
                return new List<Category>();
            }

            categories = results.ToList();

            _cache.StoreCategories(categories);

            return categories;
        }
    }
}