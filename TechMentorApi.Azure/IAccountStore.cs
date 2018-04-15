namespace TechMentorApi.Azure
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAccountStore
    {
        Task DeleteAccount(string provider, string subject, CancellationToken cancellationToken);

        Task<AccountResult> GetAccount(string provider, string subject, CancellationToken cancellationToken);
    }
}