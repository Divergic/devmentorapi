namespace TechMentorApi.Business.Queries
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IAccountQuery
    {
        Task<Account> GetAccount(User user, CancellationToken cancellationToken);
    }
}