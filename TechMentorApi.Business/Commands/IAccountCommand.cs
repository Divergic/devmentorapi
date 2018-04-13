using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TechMentorApi.Business.Commands
{
    public interface IAccountCommand
    {
        Task DeleteAccount(string username, Guid profileId, CancellationToken cancellationToken);
    }
}