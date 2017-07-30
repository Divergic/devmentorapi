namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface ICategoryStore
    {
        Task<IEnumerable<Category>> GetAllCategories(CancellationToken cancellationToken);
        
        Task StoreCategory(Category category, CancellationToken cancellationToken);
    }
}