namespace DevMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage.Table;

    public class ProfileStore : TableStoreBase, IProfileStore
    {
        private const string TableName = "Profiles";

        public ProfileStore(IStorageConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Profile> BanProfile(
            Guid accountId,
            DateTimeOffset bannedAt,
            CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();

            var partitionKey = ProfileAdapter.BuildPartitionKey(accountId);
            var rowKey = ProfileAdapter.BuildRowKey(accountId);

            var operation = TableOperation.Retrieve<ProfileAdapter>(partitionKey, rowKey);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                throw new EntityNotFoundException();
            }

            var entity = (ProfileAdapter)result.Result;

            entity.Value.BannedAt = bannedAt;

            var updateOperation = TableOperation.Replace(entity);

            await table.ExecuteAsync(updateOperation).ConfigureAwait(false);

            return entity.Value;
        }

        public async Task<Profile> GetProfile(Guid accountId, CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();

            var partitionKey = ProfileAdapter.BuildPartitionKey(accountId);
            var rowKey = ProfileAdapter.BuildRowKey(accountId);
            var operation = TableOperation.Retrieve<ProfileAdapter>(partitionKey, rowKey);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            var entity = (ProfileAdapter)result.Result;

            return entity.Value;
        }

        public Task StoreProfile(Profile profile, CancellationToken cancellationToken)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var adapter = new ProfileAdapter(profile);

            return InsertOrReplaceEntity(TableName, adapter, cancellationToken);
        }
    }
}