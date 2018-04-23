namespace TechMentorApi.Business.Queries
{
    using EnsureThat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class PhotoQuery : IPhotoQuery
    {
        private readonly IPhotoStore _store;

        public PhotoQuery(IPhotoStore store)
        {
            Ensure.Any.IsNotNull(store, nameof(store));

            _store = store;
        }

        public Task<Photo> GetPhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken)
        {
            return _store.GetPhoto(profileId, photoId, cancellationToken);
        }

        public async Task<IEnumerable<Photo>> GetPhotos(Guid profileId, CancellationToken cancellationToken)
        {
            var photoReferences = await _store.GetPhotos(profileId, cancellationToken).ConfigureAwait(false);

            var tasks = photoReferences.Select(x => _store.GetPhoto(profileId, x, cancellationToken));

            var photos = await Task.WhenAll(tasks).ConfigureAwait(false);

            return photos;
        }
    }
}