namespace DevMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IAccountStore
    {
        Task BanAccount(Guid accountId, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Account> GetAccount(string provider, string username, CancellationToken cancellationToken);

        Task RegisterAccount(NewAccount newAccount, CancellationToken cancellationToken);
    }
}