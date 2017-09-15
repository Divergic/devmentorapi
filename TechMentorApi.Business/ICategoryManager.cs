namespace TechMentorApi.Business
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    public interface ICategoryManager
    {
        Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken);

        Task<IEnumerable<Category>> GetCategories(ReadType readType, CancellationToken cancellationToken);

        Task UpdateCategory(Category category, CancellationToken cancellationToken);
    }
}