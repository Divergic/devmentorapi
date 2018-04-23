using System.Threading;
using System.Threading.Tasks;

namespace TechMentorApi.Management
{
    public interface IUserStore
    {
        Task DeleteUser(string username, CancellationToken cancellationToken);
    }
}