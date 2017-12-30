namespace TechMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IEventTrigger
    {
        Task NewCategory(Category category, CancellationToken cancellationToken);

        Task ProfileUpdated(Profile profile, CancellationToken cancellationToken);
    }
}