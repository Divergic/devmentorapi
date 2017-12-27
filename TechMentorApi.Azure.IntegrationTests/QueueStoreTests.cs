namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using Xunit;
    using Xunit.Abstractions;

    public class QueueStoreTests
    {
        private readonly ITestOutputHelper _output;

        public QueueStoreTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanCreateWithSixtyThreeCharacterQueueNameTest()
        {
            Action action = () => new QueueStore(
                Config.Storage.ConnectionString,
                "a-b-c-d-e-f-g-hij-k-l-m-n-o-p-q-r-stuv-w-x-y-z-0-1-2-3-4-5-6789");

            action.ShouldNotThrow();
        }

        [Fact]
        public void CanCreateWithThreeCharacterQueueNameTest()
        {
            Action action = () => new QueueStore(Config.Storage.ConnectionString, "a-1");

            action.ShouldNotThrow();
        }

        [Fact]
        public async Task WriteMessageCreatesQueueMessageTest()
        {
            var queueName = Guid.NewGuid().ToString("N");
            var expected = Guid.NewGuid().ToString();

            var target = new QueueStore(Config.Storage.ConnectionString, queueName);

            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);

            try
            {
                await target.WriteMessage(expected, null, null, CancellationToken.None).ConfigureAwait(false);

                var queueItem = await queue.GetMessageAsync().ConfigureAwait(false);

                var actual = queueItem.AsString;

                actual.Should().Be(expected);
            }
            finally
            {
                await queue.DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsExceptionWhenCreatedWithInvalidConnectionStringTest(string connectionString)
        {
            var queueName = Guid.NewGuid().ToString();

            Action action = () => new QueueStore(connectionString, queueName);

            var exception = action.ShouldThrow<ArgumentException>().Which;

            _output.WriteLine(exception.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("test--test")]
        [InlineData("te%st-test")]
        [InlineData("test-")]
        [InlineData("-test")]
        [InlineData("ta")]
        [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
        [InlineData("Test")]
        public void ThrowsExceptionWhenCreatedWithInvalidQueueNameTest(string queueName)
        {
            var connectionString = Guid.NewGuid().ToString();

            Action action = () => new QueueStore(connectionString, queueName);

            var exception = action.ShouldThrow<ArgumentException>().Which;

            _output.WriteLine(exception.ToString());
        }

        [Fact]
        public void WriteMessageThrowsExceptionWhenValueIsNullTest()
        {
            var queueName = Guid.NewGuid().ToString("N");

            var target = new QueueStore(Config.Storage.ConnectionString, queueName);

            Func<Task> action = async () => await target.WriteMessage(null, null, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task WriteWithNegativeVisibleInValueMakesDataImmediatelyAvailableTest()
        {
            var queueName = Guid.NewGuid().ToString("N");
            var expected = Guid.NewGuid().ToString("N");

            var target = new QueueStore(Config.Storage.ConnectionString, queueName);

            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);
            var client = storageAccount.CreateCloudQueueClient();
            var queue = client.GetQueueReference(queueName);

            try
            {
                await target.WriteMessage(expected, null, TimeSpan.FromMinutes(-1), CancellationToken.None)
                    .ConfigureAwait(false);

                var queueItem = await queue.GetMessageAsync().ConfigureAwait(false);

                var actual = queueItem.AsString;

                actual.Should().Be(expected);
            }
            finally
            {
                await queue.DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }
    }
}