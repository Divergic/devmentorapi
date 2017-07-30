namespace DevMentorApi.Azure.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class CategoryStoreTests
    {
        [Fact]
        public async Task GetAllCategoriesReturnsAllStoredCategoriesTest()
        {
            var entries = Model.Create<List<Category>>();

            var sut = new CategoryStore(Config.Storage);

            foreach (var entry in entries)
            {
                await sut.StoreCategory(entry, CancellationToken.None).ConfigureAwait(false);
            }

            var results = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);
            var actual = results.ToList();

            actual.Count.Should().BeGreaterOrEqualTo(entries.Count);

            foreach (var entry in entries)
            {
                var actualEntry = actual.FirstOrDefault(x => x.Group == entry.Group && x.Name == entry.Name);

                actualEntry.Should().NotBeNull();
                actualEntry.ShouldBeEquivalentTo(entry);
            }
        }

        [Fact]
        public async Task StoreCategoryCreatesTableAndWritesCategoryWhenTableNotFoundTest()
        {
            // Retrieve storage Category from connection-string
            var storageCategory = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageCategory.CreateCloudTableClient();

            var table = client.GetTableReference("Categories");

            await table.DeleteIfExistsAsync();

            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.FirstOrDefault(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.Should().NotBeNull();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void StoreCategoryThrowsExceptionWithNullCategoryTest()
        {
            var sut = new CategoryStore(Config.Storage);

            Func<Task> action = async () => await sut.StoreCategory(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task StoreCategoryWritesCategoryToStorageTest()
        {
            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.FirstOrDefault(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.Should().NotBeNull();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public async Task StoreCategoryWritesUpdatedCategoryToStorageTest()
        {
            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            expected.Reviewed = !expected.Reviewed;
            expected.Visible = !expected.Visible;

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.FirstOrDefault(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.Should().NotBeNull();
            actual.ShouldBeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ThrowsExceptionWhenCreatedWithInvalidConnectionStringTest(string connectionString)
        {
            var config = Substitute.For<IStorageConfiguration>();

            config.ConnectionString.Returns(connectionString);

            Action action = () => new CategoryStore(config);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigurationTest()
        {
            Action action = () => new CategoryStore(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}