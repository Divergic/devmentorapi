namespace TechMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class EventTrigger : IEventTrigger
    {
        private readonly INewCategoryQueue _newCategoryQueue;

        public EventTrigger(INewCategoryQueue newCategoryQueue)
        {
            Ensure.Any.IsNotNull(newCategoryQueue, nameof(newCategoryQueue));

            _newCategoryQueue = newCategoryQueue;
        }

        public Task NewCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(category, nameof(category));

            return _newCategoryQueue.WriteCategory(category, cancellationToken);
        }
    }
}