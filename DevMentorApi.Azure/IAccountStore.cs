namespace DevMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IAccountStore
    {
        Task<Account> GetAccount(string provider, string username, CancellationToken cancellationToken);

        Task RegisterAccount(Account account, CancellationToken cancellationToken);
    }
}