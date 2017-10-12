namespace TechMentorApi.Business.Queries
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IProfileSearchQuery
    {
        Task<IEnumerable<ProfileResult>> GetProfileResults(IEnumerable<ProfileFilter> filters,
            CancellationToken cancellationToken);
    }
}