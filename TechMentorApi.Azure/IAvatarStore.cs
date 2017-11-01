namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IAvatarStore
    {
        Task DeleteAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken);

        Task<Avatar> GetAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken);

        Task<Avatar> StoreAvatar(Avatar avatar, CancellationToken cancellationToken);
    }
}