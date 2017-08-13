namespace DevMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IProfileStore
    {
        Task<Profile> BanProfile(Guid profileId, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Profile> GetProfile(Guid profileId, CancellationToken cancellationToken);

        Task StoreProfile(Profile profile, CancellationToken cancellationToken);
    }
}