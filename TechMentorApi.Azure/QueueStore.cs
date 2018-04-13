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
    using Newtonsoft.Json;

    public abstract class QueueStore<T> : IQueueStore<T>
    {
        public static readonly Regex NameExpression = new Regex(
            "^[a-z0-9]+(-?[a-z0-9]+)*$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly string _connectionString;

        private readonly string _queueName;

        private CloudQueue _queue;

        protected QueueStore(string connectionString, string queueName)
        {
            Ensure.String.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Ensure.String.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            Ensure.Comparable.IsGt(queueName.Length, 2, nameof(queueName.Length));
            Ensure.Comparable.IsLt(queueName.Length, 64, nameof(queueName.Length));

            if (NameExpression.IsMatch(queueName) == false)
            {
                throw new ArgumentException(
                    "The queue name is invalid. See http://msdn.microsoft.com/en-us/library/windowsazure/dd179349.aspx for the validation rules of queue names.",
                    nameof(queueName));
            }

            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task WriteMessage(
            T message,
            TimeSpan? timeToLive,
            TimeSpan? visibleIn,
            CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(message, nameof(message));

            BuildQueue();

            if (visibleIn < TimeSpan.Zero)
            {
                visibleIn = null;
            }

            var messageContent = SerializeMessage(message);

            var queueMessage = new CloudQueueMessage(messageContent);

            try
            {
                await _queue.AddMessageAsync(queueMessage, timeToLive, visibleIn, null, null, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                var statusCode = (HttpStatusCode)ex.RequestInformation.HttpStatusCode;

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

        protected virtual string SerializeMessage(T message)
        {
            string messageContent;

            var stringMessage = message as string;

            if (stringMessage != null)
            {
                messageContent = stringMessage;
            }
            else
            {
                messageContent = JsonConvert.SerializeObject(message);
            }

            return messageContent;
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