namespace DevMentorApi.Business
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface ICategoryManager
    {
        Task<IEnumerable<Category>> GetCategories(ReadType readType, CancellationToken cancellationToken);

        Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken);
    }
}