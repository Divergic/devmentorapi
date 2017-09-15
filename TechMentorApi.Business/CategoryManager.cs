namespace TechMentorApi.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using EnsureThat;
    using Model;

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

            var category = new Category
            {
                Group = newCategory.Group,
                Name = newCategory.Name,
                Visible = true
            };

            await StoreCategory(category, false, cancellationToken).ConfigureAwait(false);
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

        public Task UpdateCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.That(category, nameof(category)).IsNotNull();

            return StoreCategory(category, true, cancellationToken);
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

        private async Task StoreCategory(Category category, bool mustExist, CancellationToken cancellationToken)
        {
            var existingCategory = await _store.GetCategory(category.Group, category.Name, cancellationToken)
                .ConfigureAwait(false);

            if (existingCategory != null)
            {
                category.LinkCount = existingCategory.LinkCount;
            }
            else if (mustExist)
            {
                throw new NotFoundException();
            }

            category.Reviewed = true;

            await _store.StoreCategory(category, cancellationToken).ConfigureAwait(false);

            _cache.RemoveCategories();
        }
    }
}