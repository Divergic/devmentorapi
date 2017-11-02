namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IAvatarQuery
    {
        Task<Avatar> GetAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken);
    }
}