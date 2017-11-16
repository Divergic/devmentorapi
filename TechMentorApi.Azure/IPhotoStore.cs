namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IPhotoStore
    {
        Task DeletePhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken);

        Task<Photo> GetPhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken);

        Task<PhotoDetails> StorePhoto(Photo photo, CancellationToken cancellationToken);
    }
}