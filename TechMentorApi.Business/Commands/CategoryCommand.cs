namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class CategoryCommand : ICategoryCommand
    {
        private readonly ICacheManager _cache;
        private readonly ICategoryStore _store;

        public CategoryCommand(ICategoryStore store, ICacheManager cache)
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

        public Task UpdateCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.That(category, nameof(category)).IsNotNull();

            return StoreCategory(category, true, cancellationToken);
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