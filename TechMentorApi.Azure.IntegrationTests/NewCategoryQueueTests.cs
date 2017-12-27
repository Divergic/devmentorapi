namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using ModelBuilder;
    using TechMentorApi.Model;
    using Xunit;

    public class NewCategoryQueueTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ThrowsExceptionWithInvalidConfigurationConnectionStringTest(string value)
        {
            var configuration = new StorageConfiguration
            { ConnectionString = value };

            Action action = () => new NewCategoryQueue(configuration);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigurationTest()
        {
            Action action = () => new NewCategoryQueue(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task WriteMessageCreatesQueueMessageTest()
        {
            const string queueName = "newcategories";
            var category = Model.Create<Category>();
            var expected = category.Group + Environment.NewLine + category.Name;

            var target = new NewCategoryQueue(Config.Storage);

            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);

            await target.WriteCategory(category, CancellationToken.None).ConfigureAwait(false);

            var queueItem = await queue.GetMessageAsync().ConfigureAwait(false);

            while (queueItem != null)
            {
                var actual = queueItem.AsString;

                if (actual == expected)
                {
                    // We found the item
                    return;
                }

                // Check the next queue item
                queueItem = await queue.GetMessageAsync().ConfigureAwait(false);
            }

            throw new InvalidOperationException("Expected queue item was not found.");
        }

        [Fact]
        public void WriteMessageThrowsExceptionWithNullCategoryTest()
        {
            var target = new NewCategoryQueue(Config.Storage);

            Func<Task> action = async () =>
                await target.WriteCategory(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}