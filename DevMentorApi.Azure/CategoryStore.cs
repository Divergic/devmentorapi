namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage.Table;

    public class CategoryStore : TableStoreBase, ICategoryStore
    {
        private const string TableName = "Categories";

        public CategoryStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<Category>> GetAllCategories(CancellationToken cancellationToken)
        {
            var query = new TableQuery<CategoryAdapter>();
            var table = GetTable(TableName);

            var entries = await table.ExecuteQueryAsync(query, cancellationToken).ConfigureAwait(false);

            return from x in entries
                select x.Value;
        }

        public Task StoreCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.That(category, nameof(category)).IsNotNull();

            var adapter = new CategoryAdapter(category);

            return InsertOrReplaceEntity(TableName, adapter, cancellationToken);
        }
    }
}