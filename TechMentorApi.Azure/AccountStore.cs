namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using TechMentorApi.Model;

    public class AccountStore : TableStoreBase, IAccountStore
    {
        private const string TableName = "Accounts";

        public AccountStore(IStorageConfiguration configuration)
            : base(configuration)
        {
        }

        public async Task<AccountResult> GetAccount(string provider, string username,
            CancellationToken cancellationToken)
        {
            Ensure.That(provider, nameof(provider)).IsNotNullOrWhiteSpace();
            Ensure.That(username, nameof(username)).IsNotNullOrWhiteSpace();

            var operation = TableOperation.Retrieve<AccountAdapter>(provider, username);
            var table = GetTable(TableName);

            var account = await RetrieveAccount(cancellationToken, table, operation).ConfigureAwait(false);

            if (account != null)
            {
                return account;
            }

            // This account does not yet exist
            // Attempt to create it
            var newAccount = new AccountResult
            {
                Id = Guid.NewGuid(),
                IsNewAccount = true,
                Provider = provider,
                Username = username
            };

            try
            {
                await RegisterAccount(newAccount, cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                // The account already exists between us trying to get it and create it
                // This logic avoids us writing a synchronisation lock here which would slow down the code here
                // for a situation which will only happen up to once per account
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "EntityAlreadyExists")
                {
                    // The account already exists from another operation, probably concurrent asynchronous calls
                    return await RetrieveAccount(cancellationToken, table, operation).ConfigureAwait(false);
                }

                throw;
            }

            return newAccount;
        }

        public virtual Task RegisterAccount(Account account, CancellationToken cancellationToken)
        {
            Ensure.That(account, nameof(account)).IsNotNull();

            var adapter = new AccountAdapter(account);

            // We can't use InsertOrMerge here because then the existing account would be updated with a new Id
            // We have to try to push an insert and then handle conflict responses where they record has already been created
            // Such that the existing Id can be returned
            return InsertEntity(TableName, adapter, cancellationToken);
        }

        protected virtual async Task<AccountResult> RetrieveAccount(CancellationToken cancellationToken,
            CloudTable table,
            TableOperation operation)
        {
            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            var entity = (AccountAdapter) result.Result;

            var account = new AccountResult
            {
                Id = entity.Value.Id,
                IsNewAccount = false,
                Provider = entity.Value.Provider,
                Username = entity.Value.Username
            };

            return account;
        }
    }
}