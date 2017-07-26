namespace DevMentorApi.Azure
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AccountStore : TableStoreBase, IAccountStore
    {
        private const string TableName = "Accounts";

        public AccountStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task BanAccount(Guid accountId, DateTimeOffset bannedAt, CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();

            var filter = TableQuery.GenerateFilterConditionForGuid(nameof(Account.Id), QueryComparisons.Equal, accountId);
            var query = new TableQuery<AccountAdapter>().Where(filter);
            var table = GetTable(TableName);

            var results = await table.ExecuteQueryAsync(query, cancellationToken).ConfigureAwait(false);

            var result = results.SingleOrDefault();

            if (result == null)
            {
                throw new EntityNotFoundException();
            }

            result.Value.BannedAt = bannedAt;

            var updateOperation = TableOperation.Replace(result);

            await table.ExecuteAsync(updateOperation).ConfigureAwait(false);
        }

        public async Task<Account> GetAccount(string provider, string username, CancellationToken cancellationToken)
        {
            Ensure.That(provider, nameof(provider)).IsNotNullOrWhiteSpace();
            Ensure.That(username, nameof(username)).IsNotNullOrWhiteSpace();

            var operation = TableOperation.Retrieve<AccountAdapter>(provider, username);
            var table = GetTable(TableName);

            try
            {
                var result = await table.ExecuteAsync(operation, null, null, cancellationToken)
                    .ConfigureAwait(false);

                var entity = result?.Result as AccountAdapter;

                return entity?.Value;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task RegisterAccount(NewAccount newAccount, CancellationToken cancellationToken)
        {
            Ensure.That(newAccount, nameof(newAccount)).IsNotNull();

            var account = new Account(newAccount);
            var adapter = new AccountAdapter(account);
            var table = GetTable(TableName);
            var operation = TableOperation.Insert(adapter);

            try
            {
                await table.ExecuteAsync(operation).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    // The table doesn't exist yet, retry
                    await table.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);

                    await table.ExecuteAsync(operation).ConfigureAwait(false);

                    return;
                }

                throw;
            }
        }
    }
}