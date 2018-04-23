namespace TechMentorApi.Azure
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProfileStore
    {
        Task<Profile> BanProfile(Guid profileId, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Profile> DeleteProfile(Guid profileId, CancellationToken cancellationToken);

        Task<Profile> GetProfile(Guid profileId, CancellationToken cancellationToken);

        Task<IEnumerable<ProfileResult>> GetProfileResults(CancellationToken cancellationToken);

        Task StoreProfile(Profile profile, CancellationToken cancellationToken);
    }
}