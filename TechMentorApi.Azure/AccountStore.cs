namespace TechMentorApi.Azure
{
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public class AccountStore : TableStoreBase, IAccountStore
    {
        private const string TableName = "Accounts";

        public AccountStore(IStorageConfiguration configuration)
            : base(configuration)
        {
        }

        public async Task DeleteAccount(string provider, string subject, CancellationToken cancellationToken)
        {
            Ensure.String.IsNotNullOrWhiteSpace(provider, nameof(provider));
            Ensure.String.IsNotNullOrWhiteSpace(subject, nameof(subject));

            var retrieveOperation = TableOperation.Retrieve<AccountAdapter>(provider, subject);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(retrieveOperation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return;
            }

            var entity = (AccountAdapter)result.Result;

            var deleteOperation = TableOperation.Delete(entity);

            await table.ExecuteAsync(deleteOperation).ConfigureAwait(false);
        }

        public async Task<AccountResult> GetAccount(string provider, string subject,
            CancellationToken cancellationToken)
        {
            Ensure.String.IsNotNullOrWhiteSpace(provider, nameof(provider));
            Ensure.String.IsNotNullOrWhiteSpace(subject, nameof(subject));

            var operation = TableOperation.Retrieve<AccountAdapter>(provider, subject);
            var table = GetTable(TableName);

            var account = await RetrieveAccount(cancellationToken, table, operation).ConfigureAwait(false);

            if (account != null)
            {
                return account;
            }

            try
            {
                // This account does not yet exist Attempt to create it
                var newAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    Provider = provider,
                    Subject = subject
                };

                await RegisterAccount(newAccount, cancellationToken).ConfigureAwait(false);

                account = new AccountResult(newAccount)
                {
                    IsNewAccount = true
                };
            }
            catch (StorageException ex)
            {
                // The account already exists between us trying to get it and create it This logic
                // avoids us writing a synchronisation lock here which would slow down the code here
                // for a situation which will only happen up to once per account
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "EntityAlreadyExists")
                {
                    // The account already exists from another operation, probably concurrent
                    // asynchronous calls
                    return await RetrieveAccount(cancellationToken, table, operation).ConfigureAwait(false);
                }

                throw;
            }

            return account;
        }

        protected virtual Task RegisterAccount(Account account, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(account, nameof(account));

            var adapter = new AccountAdapter(account);

            // We can't use InsertOrMerge here because then the existing account would be updated
            // with a new Id We have to try to push an insert and then handle conflict responses
            // where they record has already been created Such that the existing Id can be returned
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

            var entity = (AccountAdapter)result.Result;

            var account = new AccountResult(entity.Value)
            {
                IsNewAccount = false
            };

            return account;
        }
    }
}