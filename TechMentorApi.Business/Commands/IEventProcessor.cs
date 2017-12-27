namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IEventProcessor
    {
        Task NewCategoryAdded(Category category, CancellationToken cancellationToken);
    }
}