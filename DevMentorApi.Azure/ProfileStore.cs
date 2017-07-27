namespace DevMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class ProfileStore : TableStoreBase, IProfileStore
    {
        private const string TableName = "Profiles";

        public ProfileStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Profile> GetProfile(Guid accountId, CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();

            var rowKey = accountId.ToString();
            var partitionKey = rowKey.Substring(0, 1);
            var operation = TableOperation.Retrieve<ProfileAdapter>(partitionKey, rowKey);
            var table = GetTable(TableName);

            try
            {
                var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

                var entity = result?.Result as ProfileAdapter;

                return entity?.Value;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task StoreProfile(Profile profile, CancellationToken cancellationToken)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var adapter = new ProfileAdapter(profile);
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