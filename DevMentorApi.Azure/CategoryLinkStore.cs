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
                var link = new CategoryLink
                {
                    CategoryGroup = categoryGroup,
                    CategoryName = categoryName,
                    ProfileId = change.ProfileId
                };
                var adapter = new CategoryLinkAdapter(link);

                if (change.ChangeType == CategoryLinkChangeType.Add)
                {
                    var operation = TableOperation.InsertOrReplace(adapter);

                    batch.Add(operation);
                }
                else
                {
                    // We don't care about concurrency here because we are removing the item
                    adapter.ETag = "*";

                    var operation = TableOperation.Delete(adapter);

                    batch.Add(operation);
                }
            }

            if (batch.Count == 0)
            {
                // We were provided a changes instance but no changes to be made
                return;
            }

            var context = new OperationContext();

            try
            {
                await table.ExecuteBatchAsync(batch, null, context).ConfigureAwait(false);
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

                throw;
            }
        }
    }
}