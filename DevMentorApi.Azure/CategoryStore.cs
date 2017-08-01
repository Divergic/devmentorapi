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

    public class CategoryStore : TableStoreBase, ICategoryStore
    {
        private const string TableName = "Categories";

        public CategoryStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task CreateCategory(CategoryGroup group, string name, CancellationToken cancellationToken)
        {
            Ensure.That(name, nameof(name)).IsNotNullOrWhiteSpace();

            var category = new Category
            {
                Group = group,
                Name = name
            };

            var adapter = new CategoryAdapter(category);
            var operation = TableOperation.Insert(adapter);

            try
            {
                await ExecuteWithCreateTable(TableName, operation, cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    // This category already exists
                    // A user has tried to create a category that already exists
                    // We might have hit this because the API cache was out of date
                    // such that the category was already written to storage, but the API didn't think it was there
                    // The outcome is that the category exists either way which is what we want
                    return;
                }

                throw;
            }
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