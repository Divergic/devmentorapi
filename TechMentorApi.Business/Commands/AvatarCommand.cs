namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class AvatarCommand : IAvatarCommand
    {
        private readonly IAvatarStore _store;

        public AvatarCommand(IAvatarStore store)
        {
            Ensure.That(store, nameof(store)).IsNotNull();

            _store = store;
        }

        public Task<Avatar> CreateAvatar(Avatar avatar, CancellationToken cancellationToken)
        {
            Ensure.That(avatar, nameof(avatar)).IsNotNull();

            return _store.StoreAvatar(avatar, cancellationToken);
        }
    }
}