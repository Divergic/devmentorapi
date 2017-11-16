namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IPhotoQuery
    {
        Task<Photo> GetPhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken);
    }
}