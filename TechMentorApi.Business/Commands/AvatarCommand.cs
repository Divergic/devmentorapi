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
        private readonly IAvatarConfig _config;

        public AvatarCommand(IAvatarStore store, IAvatarConfig config)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _store = store;
            _config = config;
        }

        public Task<Avatar> CreateAvatar(Avatar avatar, CancellationToken cancellationToken)
        {
            Ensure.That(avatar, nameof(avatar)).IsNotNull();

            // Resize the image to the maximum boundaries

            return _store.StoreAvatar(avatar, cancellationToken);
        }
    }
}