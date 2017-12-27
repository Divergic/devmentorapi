namespace TechMentorApi.Azure
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class QueueStore : IQueueStore
    {
        private static readonly Regex _nameExpression = new Regex(
            "^[a-z0-9]+(-?[a-z0-9]+)*$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly string _connectionString;

        private readonly string _queueName;

        private CloudQueue _queue;

        public QueueStore(string connectionString, string queueName)
        {
            Ensure.String.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Ensure.String.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            Ensure.That(queueName.Length, nameof(queueName.Length)).IsGt(2);
            Ensure.That(queueName.Length, nameof(queueName.Length)).IsLt(64);

            if (_nameExpression.IsMatch(queueName) == false)
            {
                throw new ArgumentException(
                    "The queue name is invalid. See http://msdn.microsoft.com/en-us/library/windowsazure/dd179349.aspx for the validation rules of queue names.",
                    nameof(queueName));
            }

            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task WriteMessage(
            string message,
            TimeSpan? timeToLive,
            TimeSpan? visibleIn,
            CancellationToken cancellationToken)
        {
            Ensure.String.IsNotNullOrWhiteSpace(message, nameof(message));

            BuildQueue();

            if (visibleIn < TimeSpan.Zero)
            {
                visibleIn = null;
            }

            var queueMessage = new CloudQueueMessage(message);

            try
            {
                await _queue.AddMessageAsync(queueMessage, timeToLive, visibleIn, null, null, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                var statusCode = (HttpStatusCode) ex.RequestInformation.HttpStatusCode;

                if (statusCode == HttpStatusCode.NotFound ||
                    statusCode == HttpStatusCode.BadRequest)
                {
                    // Assume the queue does not exist
                    await _queue.CreateIfNotExistsAsync(null, null, cancellationToken).ConfigureAwait(false);

                    await _queue.AddMessageAsync(queueMessage, timeToLive, visibleIn, null, null, cancellationToken)
                        .ConfigureAwait(false);

                    return;
                }

                throw;
            }
        }

        private void BuildQueue()
        {
            if (_queue != null)
            {
                return;
            }

            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(_queueName);

            _queue = queue;
        }
    }
}