namespace DevMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    public interface IProfileManager
    {
        Task<Profile> BanProfile(Guid id, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken);

        Task<PublicProfile> GetPublicProfile(Guid id, CancellationToken cancellationToken);

        Task UpdateProfile(Guid profileId, UpdatableProfile profile, CancellationToken cancellationToken);
    }
}