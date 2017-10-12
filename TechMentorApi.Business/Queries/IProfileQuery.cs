namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IProfileQuery
    {
        Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken);

        Task<PublicProfile> GetPublicProfile(Guid id, CancellationToken cancellationToken);
    }
}