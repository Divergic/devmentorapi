namespace TechMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IAccountManager
    {
        Task<Account> GetAccount(User user, CancellationToken cancellationToken);
    }
}