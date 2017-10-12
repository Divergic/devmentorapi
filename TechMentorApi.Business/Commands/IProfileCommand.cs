namespace TechMentorApi.Business.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IProfileCommand
    {
        Task<Profile> BanProfile(Guid id, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task UpdateProfile(Guid profileId, UpdatableProfile profile, CancellationToken cancellationToken);
    }
}