namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class AvatarCommand : IAvatarCommand
    {
        private readonly IAvatarConfig _config;
        private readonly IAvatarResizer _resizer;
        private readonly IAvatarStore _store;

        public AvatarCommand(IAvatarStore store, IAvatarResizer resizer, IAvatarConfig config)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(resizer, nameof(resizer)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _store = store;
            _resizer = resizer;
            _config = config;
        }

        public async Task<AvatarDetails> CreateAvatar(Avatar avatar, CancellationToken cancellationToken)
        {
            Ensure.That(avatar, nameof(avatar)).IsNotNull();

            using (var updatedAvatar = _resizer.Resize(avatar, _config.MaxHeight, _config.MaxWidth))
            {
                // Need async here so that the updated avatar can be disposed
                var avatarDetails = await _store.StoreAvatar(updatedAvatar, cancellationToken).ConfigureAwait(false);

                return avatarDetails;
            }
        }
    }
}