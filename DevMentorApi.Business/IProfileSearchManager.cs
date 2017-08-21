namespace DevMentorApi.Business
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    public interface IProfileSearchManager
    {
        Task<IEnumerable<ProfileResult>> GetProfileResults(IEnumerable<ProfileFilter> filters,
            CancellationToken cancellationToken);
    }
}