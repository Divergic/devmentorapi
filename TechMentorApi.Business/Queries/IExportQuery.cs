using System;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Model;

namespace TechMentorApi.Business.Queries
{
    public interface IExportQuery
    {
        Task<ExportProfile> GetExportProfile(Guid profileId, CancellationToken cancellationToken);
    }
}