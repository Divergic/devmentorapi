namespace DevMentorApi.Azure
{
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

        protected CloudTable GetTable(string tableName)
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(_configuration.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            return client.GetTableReference(tableName);
        }
    }
}