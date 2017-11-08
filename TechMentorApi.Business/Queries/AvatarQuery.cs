namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class AvatarQuery : IAvatarQuery
    {
        private readonly IAvatarStore _store;

        public AvatarQuery(IAvatarStore store)
        {
            Ensure.That(store, nameof(store)).IsNotNull();

            _store = store;
        }

        public Task<Avatar> GetAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken)
        {
            return _store.GetAvatar(profileId, avatarId, cancellationToken);
        }
    }
}