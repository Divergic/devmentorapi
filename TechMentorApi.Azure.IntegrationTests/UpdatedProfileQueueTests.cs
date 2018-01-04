namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class UpdatedProfileQueueTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ThrowsExceptionWithInvalidConfigurationConnectionStringTest(string value)
        {
            var configuration = new StorageConfiguration
            {
                ConnectionString = value
            };

            Action action = () => new UpdatedProfileQueue(configuration);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigurationTest()
        {
            Action action = () => new UpdatedProfileQueue(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}