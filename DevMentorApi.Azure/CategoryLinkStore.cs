namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
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
            var partitionKey = CategoryLinkAdapter.BuildPartitionKey(categoryGroup, categoryName);
            var partitionKeyFilter =
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<CategoryLinkAdapter>().Where(partitionKeyFilter);
            var table = GetTable(TableName);

            var entries = await table.ExecuteQueryAsync(query, cancellationToken).ConfigureAwait(false);

            return from x in entries
                select x.Value;
        }

        public Task StoreCategoryLinks(
            CategoryGroup categoryGroup,
            string categoryName,
            IEnumerable<CategoryLinkChange> changes,
            CancellationToken cancellationToken)
        {
            return TODO_IMPLEMENT_ME;
        }
    }
}