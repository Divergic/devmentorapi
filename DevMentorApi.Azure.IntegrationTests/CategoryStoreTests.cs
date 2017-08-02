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
        public async Task CreateCategoryIgnoresExistingCategoryTest()
        {
            var expected = Model.Create<Category>().Set(x => x.Reviewed = true).Set(x => x.Visible = true)
                .Set(x => x.LinkCount = Math.Abs(Environment.TickCount));

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            await sut.CreateCategory(expected.Group, expected.Name, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.Single(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateCategoryStoresNewCategoryTest()
        {
            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.CreateCategory(expected.Group, expected.Name, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            categories.Should().Contain(x => x.Group == expected.Group && x.Name == expected.Name);
        }

        [Fact]
        public async Task CreateCategoryStoresNewCategoryWithReviewedAndVisibleAsFalseAndZeroLinkCountTest()
        {
            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.CreateCategory(expected.Group, expected.Name, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.Single(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.Reviewed.Should().BeFalse();
            actual.Visible.Should().BeFalse();
            actual.LinkCount.Should().Be(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CreateCategoryThrowsExceptionWithInvalidNameTest(string name)
        {
            var sut = new CategoryStore(Config.Storage);

            Func<Task> action = async () => await sut.CreateCategory(CategoryGroup.Gender, name, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(110)]
        public async Task GetAllCategoriesReturnsCategoriesWithDifferentBatchSizesTest(int itemCount)
        {
            var builder = Model.BuildStrategy.Clone();

            builder.TypeCreators.OfType<EnumerableTypeCreator>().Single().AutoPopulateCount = itemCount;

            var entries = Model.Create<List<Category>>();

            entries.Count.Should().Be(itemCount);

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
                var actualEntry = actual.First(x => x.Group == entry.Group && x.Name == entry.Name);

                actualEntry.Should().NotBeNull();
                actualEntry.ShouldBeEquivalentTo(entry);
            }
        }

        [Fact]
        public async Task GetAllCategoriesReturnsEmptyWhenTableNotFoundTest()
        {
            // Retrieve storage Category from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("Categories");

            await table.DeleteIfExistsAsync();

            var sut = new CategoryStore(Config.Storage);

            var actual = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task StoreCategoryCreatesTableAndWritesCategoryWhenTableNotFoundTest()
        {
            // Retrieve storage Category from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("Categories");

            await table.DeleteIfExistsAsync();

            var expected = Model.Create<Category>();

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(expected, CancellationToken.None).ConfigureAwait(false);

            var categories = await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false);

            var actual = categories.First(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public async Task StoreCategoryOverwritesCategoryWithCaseInsensitiveNameMatchTest()
        {
            var first = Model.Create<Category>().Set(x => x.Name = x.Name.ToUpperInvariant());
            var second = new Category
            {
                Group = first.Group,
                Name = first.Name.ToLowerInvariant(),
                Reviewed = !first.Reviewed,
                Visible = !first.Reviewed
            };

            var sut = new CategoryStore(Config.Storage);

            await sut.StoreCategory(first, CancellationToken.None).ConfigureAwait(false);
            await sut.StoreCategory(second, CancellationToken.None).ConfigureAwait(false);

            var categories = (await sut.GetAllCategories(CancellationToken.None).ConfigureAwait(false)).ToList();

            var actual = categories.First(
                x => x.Group == first.Group && x.Name.Equals(first.Name, StringComparison.OrdinalIgnoreCase));

            actual.ShouldBeEquivalentTo(second);

            // The first entry should have been replaced with the second one based on the case insensitive name match (name is the rowkey)
            categories.Should().NotContain(
                x => x.Group == second.Group && x.Name.Equals(first.Name, StringComparison.Ordinal));
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

            var actual = categories.First(x => x.Group == expected.Group && x.Name == expected.Name);

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

            var actual = categories.First(x => x.Group == expected.Group && x.Name == expected.Name);

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