namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface ICategoryCommand
    {
        Task CreateCategory(NewCategory newCategory, CancellationToken cancellationToken);

        Task UpdateCategory(Category category, CancellationToken cancellationToken);
    }
}