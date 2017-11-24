namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class PhotoCommand : IPhotoCommand
    {
        private readonly IPhotoConfig _config;
        private readonly IPhotoResizer _resizer;
        private readonly IPhotoStore _store;

        public PhotoCommand(IPhotoStore store, IPhotoResizer resizer, IPhotoConfig config)
        {
            Ensure.Any.IsNotNull(store, nameof(store));
            Ensure.Any.IsNotNull(resizer, nameof(resizer));
            Ensure.Any.IsNotNull(config, nameof(config));

            _store = store;
            _resizer = resizer;
            _config = config;
        }

        public async Task<PhotoDetails> CreatePhoto(Photo photo, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(photo, nameof(photo));

            using (var updatedPhoto = _resizer.Resize(photo, _config.MaxHeight, _config.MaxWidth))
            {
                // Need async here so that the updated photo can be disposed
                var photoDetails = await _store.StorePhoto(updatedPhoto, cancellationToken).ConfigureAwait(false);

                return photoDetails;
            }
        }
    }
}