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
        private readonly IUpdatedProfileQueue _updatedProfileQueue;

        public EventTrigger(INewCategoryQueue newCategoryQueue, IUpdatedProfileQueue updatedProfileQueue)
        {
            Ensure.Any.IsNotNull(newCategoryQueue, nameof(newCategoryQueue));
            Ensure.Any.IsNotNull(updatedProfileQueue, nameof(updatedProfileQueue));

            _newCategoryQueue = newCategoryQueue;
            _updatedProfileQueue = updatedProfileQueue;
        }

        public Task NewCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(category, nameof(category));

            return _newCategoryQueue.WriteMessage(category, cancellationToken);
        }

        public Task ProfileUpdated(Profile profile, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));

            return _updatedProfileQueue.WriteMessage(profile, cancellationToken);
        }
    }
}