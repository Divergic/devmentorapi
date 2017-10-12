namespace TechMentorApi.Business.Queries
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class CategoryQuery : ICategoryQuery
    {
        private readonly ICacheManager _cache;
        private readonly ICategoryStore _store;

        public CategoryQuery(ICategoryStore store, ICacheManager cache)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _store = store;
            _cache = cache;
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