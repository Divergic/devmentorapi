namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public class EventProcessor : IEventProcessor
    {
        public Task NewCategoryAdded(Category category, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}