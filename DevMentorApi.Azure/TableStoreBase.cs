namespace DevMentorApi.Azure
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public abstract class TableStoreBase
    {
        private readonly IStorageConfiguration _configuration;

        protected TableStoreBase(IStorageConfiguration configuration)
        {
            Ensure.That(configuration, nameof(configuration)).IsNotNull();
            Ensure.That(configuration.ConnectionString, nameof(configuration.ConnectionString)).IsNotNullOrWhiteSpace();

            _configuration = configuration;
        }

        protected async Task ExecuteWithCreateTable(
            string tableName,
            TableOperation operation,
            CancellationToken cancellationToken)
        {
            var table = GetTable(tableName);

            try
            {
                await table.ExecuteAsync(operation).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                if (operation.OperationType == TableOperationType.Delete)
                {
                    // We can't delete what isn't there, but the outcome is the desired one
                    return;
                }

                // The table doesn't exist yet, retry
                await table.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);

                await table.ExecuteAsync(operation).ConfigureAwait(false);
            }
        }

        protected CloudTable GetTable(string tableName)
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(_configuration.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            return client.GetTableReference(tableName);
        }

        protected Task InsertEntity(string tableName, ITableEntity entity, CancellationToken cancellationToken)
        {
            var operation = TableOperation.Insert(entity);

            return ExecuteWithCreateTable(tableName, operation, cancellationToken);
        }

        protected Task InsertOrReplaceEntity(string tableName, ITableEntity entity, CancellationToken cancellationToken)
        {
            var operation = TableOperation.InsertOrReplace(entity);

            return ExecuteWithCreateTable(tableName, operation, cancellationToken);
        }
    }
}