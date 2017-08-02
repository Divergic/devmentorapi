namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class CategoryLinkStore : TableStoreBase, ICategoryLinkStore
    {
        private const string TableName = "CategoryLinks";

        public CategoryLinkStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<CategoryLink>> GetCategoryLinks(
            CategoryGroup categoryGroup,
            string categoryName,
            CancellationToken cancellationToken)
        {
            Ensure.That(categoryName, nameof(categoryName)).IsNotNullOrWhiteSpace();

            var partitionKey = CategoryLinkAdapter.BuildPartitionKey(categoryGroup, categoryName);
            var partitionKeyFilter =
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<CategoryLinkAdapter>().Where(partitionKeyFilter);
            var table = GetTable(TableName);
            
            var entries = await table.ExecuteQueryAsync(query, cancellationToken).ConfigureAwait(false);

            return from x in entries
                select x.Value;
        }

        public async Task StoreCategoryLinks(
            CategoryGroup categoryGroup,
            string categoryName,
            IEnumerable<CategoryLinkChange> changes,
            CancellationToken cancellationToken)
        {
            Ensure.That(categoryName, nameof(categoryName)).IsNotNullOrWhiteSpace();
            Ensure.That(changes, nameof(changes)).IsNotNull();

            var table = GetTable(TableName);

            var batch = new TableBatchOperation();

            foreach (var change in changes)
            {
                if (batch.Count == 100)
                {
                    // Batches can only handle 100 items, need to execute this batch
                    await ExecuteBatch(table, batch, cancellationToken);

                    batch.Clear();
                }

                var operation = BuildLinkChangeTableOperation(categoryGroup, categoryName, change);

                batch.Add(operation);
            }

            if (batch.Count == 0)
            {
                // We were provided a changes instance but no changes to be made
                return;
            }
            
            await ExecuteBatch(table, batch, cancellationToken);
        }

        private async Task ExecuteBatch(CloudTable table, TableBatchOperation batch, CancellationToken cancellationToken)
        {
            try
            {
                await table.ExecuteBatchAsync(batch).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                // Check if this 404 is because of the table not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "TableNotFound")
                {
                    // The table doesn't exist yet, retry
                    await table.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);

                    await table.ExecuteBatchAsync(batch).ConfigureAwait(false);

                    return;
                }

                // Most likely this is because we are trying to delete an entity that does not exist
                // Because this has failed, none of the other potentially valid changes have been actioned in the batch
                // The only way to recover from this is to retry each item individually and forgo the transaction saving of batching
                // changes to a storage partition
                var tasks = batch.Select(x => ExecuteWithCreateTable(table, x, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private static TableOperation BuildLinkChangeTableOperation(
            CategoryGroup categoryGroup,
            string categoryName,
            CategoryLinkChange change)
        {
            var link = new CategoryLink
            {
                CategoryGroup = categoryGroup,
                CategoryName = categoryName,
                ProfileId = change.ProfileId
            };
            var adapter = new CategoryLinkAdapter(link);
            TableOperation operation;

            if (change.ChangeType == CategoryLinkChangeType.Add)
            {
                operation = TableOperation.InsertOrReplace(adapter);
            }
            else
            {
                // We don't care about concurrency here because we are removing the item
                adapter.ETag = "*";

                operation = TableOperation.Delete(adapter);
            }
            return operation;
        }
    }
}