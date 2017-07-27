namespace DevMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IAccountManager
    {
        Task BanAccount(Guid accountId, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Account> GetAccount(User user, CancellationToken cancellationToken);
    }
}