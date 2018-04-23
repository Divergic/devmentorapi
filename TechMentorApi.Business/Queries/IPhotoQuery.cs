namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IPhotoQuery
    {
        Task<Photo> GetPhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken);

        Task<IEnumerable<Photo>> GetPhotos(Guid profileId, CancellationToken cancellationToken);
    }
}