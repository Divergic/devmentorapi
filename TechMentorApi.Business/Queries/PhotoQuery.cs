namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
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
    }
}