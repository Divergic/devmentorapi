namespace DevMentorApi.Azure
{
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

        public async Task<Account> GetAccount(string provider, string username, CancellationToken cancellationToken)
        {
            Ensure.That(provider, nameof(provider)).IsNotNullOrWhiteSpace();
            Ensure.That(username, nameof(username)).IsNotNullOrWhiteSpace();

            var operation = TableOperation.Retrieve<AccountAdapter>(provider, username);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            var entity = (AccountAdapter)result.Result;

            return entity.Value;
        }

        public async Task RegisterAccount(Account account, CancellationToken cancellationToken)
        {
            Ensure.That(account, nameof(account)).IsNotNull();

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