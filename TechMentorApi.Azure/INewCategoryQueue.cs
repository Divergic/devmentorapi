namespace TechMentorApi.Azure
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface INewCategoryQueue
    {
        Task WriteCategory(Category category, CancellationToken cancellationToken);
    }
}