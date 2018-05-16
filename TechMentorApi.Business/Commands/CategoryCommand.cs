namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class CategoryCommand : ICategoryCommand
    {
        private readonly ICategoryCache _cache;
        private readonly ICategoryStore _store;

        public CategoryCommand(ICategoryStore store, ICategoryCache cache)
        {
            Ensure.Any.IsNotNull(store, nameof(store));
            Ensure.Any.IsNotNull(cache, nameof(cache));

            _store = store;
            _cache = cache;
        }

        public async Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(newCategory, nameof(newCategory));

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
            Ensure.Any.IsNotNull(category, nameof(category));

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
            _cache.RemoveCategory(category);
        }
    }
}