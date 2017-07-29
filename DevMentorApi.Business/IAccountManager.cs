namespace DevMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IAccountManager
    {
        Task<Account> GetAccount(User user, CancellationToken cancellationToken);
    }
}