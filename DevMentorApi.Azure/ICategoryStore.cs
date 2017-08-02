namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface ICategoryStore
    {
        Task CreateCategory(CategoryGroup group, string name, CancellationToken cancellationToken);

        Task<IEnumerable<Category>> GetAllCategories(CancellationToken cancellationToken);

        Task StoreCategory(Category category, CancellationToken cancellationToken);
    }
}