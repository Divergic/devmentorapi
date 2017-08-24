namespace TechMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface ICategoryStore
    {
        Task CreateCategory(CategoryGroup group, string name, CancellationToken cancellationToken);

        Task<IEnumerable<Category>> GetAllCategories(CancellationToken cancellationToken);

        Task<Category> GetCategory(CategoryGroup group, string name, CancellationToken cancellationToken);

        Task StoreCategory(Category category, CancellationToken cancellationToken);
    }
}