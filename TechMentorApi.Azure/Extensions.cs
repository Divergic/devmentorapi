namespace TechMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public static class Extensions
    {
        public static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity, new()
        {
            var items = new List<T>(100);

            try
            {
                TableContinuationToken token = null;

                do
                {
                    var segment = await table.ExecuteQuerySegmentedAsync(query, token, null, null, cancellationToken).ConfigureAwait(false);

                    if (segment == null)
                    {
                        break;
                    }

                    token = segment.ContinuationToken;
                    items.AddRange(segment);
                }
                while (token != null &&
                       cancellationToken.IsCancellationRequested == false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                return items;
            }

            return items;
        }
    }
}