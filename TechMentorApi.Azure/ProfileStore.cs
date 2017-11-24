namespace TechMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage.Table;
    using Model;

    public class ProfileStore : TableStoreBase, IProfileStore
    {
        private const string TableName = "Profiles";
        private static readonly IList<string> _resultsColumns = DetermineProfileResultColumns();

        public ProfileStore(IStorageConfiguration configuration)
            : base(configuration)
        {
        }

        public async Task<Profile> BanProfile(
            Guid profileId,
            DateTimeOffset bannedAt,
            CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));

            var partitionKey = ProfileAdapter.BuildPartitionKey(profileId);
            var rowKey = ProfileAdapter.BuildRowKey(profileId);

            var operation = TableOperation.Retrieve<ProfileAdapter>(partitionKey, rowKey);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            var entity = (ProfileAdapter) result.Result;

            if (entity.Value.BannedAt.HasValue)
            {
                // This profile has already been banned
                return null;
            }

            entity.Value.BannedAt = bannedAt;

            var updateOperation = TableOperation.Replace(entity);

            await table.ExecuteAsync(updateOperation).ConfigureAwait(false);

            return entity.Value;
        }

        public async Task<Profile> GetProfile(Guid profileId, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));

            var partitionKey = ProfileAdapter.BuildPartitionKey(profileId);
            var rowKey = ProfileAdapter.BuildRowKey(profileId);
            var operation = TableOperation.Retrieve<ProfileAdapter>(partitionKey, rowKey);
            var table = GetTable(TableName);

            var result = await table.ExecuteAsync(operation, null, null, cancellationToken).ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            var entity = (ProfileAdapter) result.Result;

            return entity.Value;
        }

        public async Task<IEnumerable<ProfileResult>> GetProfileResults(CancellationToken cancellationToken)
        {
            var table = GetTable(TableName);

            // We can't filter out BannedAt because the filter would search for where the value is null
            // This is not possible with the query mechanics of Azure table services so this filter has 
            // to happen in the code here rather than in the table service itself
            var statusFilter = TableQuery.GenerateFilterCondition(nameof(Profile.Status), QueryComparisons.NotEqual,
                ProfileStatus.Hidden.ToString());

            var query = new TableQuery<ProfileResultAdapter>
            {
                // For performance reasons, select the minimum number of columns required to filter and populate profile results
                SelectColumns = _resultsColumns,
                FilterString = statusFilter
            };

            var results = await table.ExecuteQueryAsync(query, cancellationToken).ConfigureAwait(false);

            return from x in results
                where x.BannedAt == null
                select x.Value;
        }

        public Task StoreProfile(Profile profile, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));

            var adapter = new ProfileAdapter(profile);

            return InsertOrReplaceEntity(TableName, adapter, cancellationToken);
        }

        private static IList<string> DetermineProfileResultColumns()
        {
            var properties = typeof(ProfileResult).GetTypeInfo().GetProperties();

            var columns = properties.Select(x => x.Name).ToList();

            // Add the BannedAt field because it will be exposed by the adapter for filtering, but not the model returned by the adapter
            columns.Add(nameof(ProfileResultAdapter.BannedAt));

            return columns;
        }
    }
}