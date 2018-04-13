using System;
using System.Threading;
using System.Threading.Tasks;

namespace TechMentorApi.Business.Commands
{
    public class AccountCommand : IAccountCommand
    {
        public Task DeleteAccount(string username, Guid profileId, CancellationToken cancellationToken)
        {
            // Remove profile photos
            // Remove profile
            // Remove account
            // Remove account from Auth0

            throw new NotImplementedException();
        }
    }
}