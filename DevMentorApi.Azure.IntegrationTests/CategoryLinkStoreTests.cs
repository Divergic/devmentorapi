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
    using Xunit;

    public class CategoryLinkStoreTests
    {
        [Fact]
        public async Task GetCategoryLinksReturnsEmptyWhenNoItemsFoundTest()
        {
            var categoryName = Guid.NewGuid().ToString();

            var sut = new CategoryLinkStore(Config.Storage);

            var actual = await sut.GetCategoryLinks(CategoryGroup.Gender, categoryName, CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCategoryLinksReturnsEmptyWhenTableNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("CategoryLinks");

            await table.DeleteIfExistsAsync();

            var categoryName = Guid.NewGuid().ToString();

            var sut = new CategoryLinkStore(Config.Storage);

            var actual = await sut.GetCategoryLinks(CategoryGroup.Gender, categoryName, CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetCategoryLinksThrowsExceptionWithInvalidCategoryNameTest(string categoryName)
        {
            var sut = new CategoryLinkStore(Config.Storage);

            Func<Task> action = async () => await sut
                .GetCategoryLinks(CategoryGroup.Gender, categoryName, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StoreCategoryLinksAddsMultipleItemsTest()
        {
            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = Model.Create<List<CategoryLinkChange>>()
                .SetEach(x => x.ChangeType = CategoryLinkChangeType.Add);

            var sut = new CategoryLinkStore(Config.Storage);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            var actual = (await sut.GetCategoryLinks(Group, categoryName, CancellationToken.None).ConfigureAwait(false))
                .ToList();

            actual.All(x => x.CategoryGroup == Group).Should().BeTrue();
            actual.All(x => x.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            actual.ShouldAllBeEquivalentTo(changes, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task StoreCategoryLinksAddsNewItemsWhenRemovingItemsInBatchFailsTest()
        {
            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = new List<CategoryLinkChange>
            {
                new CategoryLinkChange
                {
                    ChangeType = CategoryLinkChangeType.Remove,
                    ProfileId = Guid.NewGuid()
                },
                new CategoryLinkChange
                {
                    ChangeType = CategoryLinkChangeType.Add,
                    ProfileId = Guid.NewGuid()
                },
                new CategoryLinkChange
                {
                    ChangeType = CategoryLinkChangeType.Remove,
                    ProfileId = Guid.NewGuid()
                }
            };

            var sut = new CategoryLinkStore(Config.Storage);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            var actual = (await sut.GetCategoryLinks(Group, categoryName, CancellationToken.None).ConfigureAwait(false))
                .ToList();

            actual.Should().HaveCount(1);
            actual.All(x => x.CategoryGroup == Group).Should().BeTrue();
            actual.All(x => x.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            actual.ShouldAllBeEquivalentTo(
                changes.Where(x => x.ChangeType == CategoryLinkChangeType.Add),
                opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task StoreCategoryLinksAddsNewItemsWhenTableNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the table client
            var client = storageAccount.CreateCloudTableClient();

            var table = client.GetTableReference("CategoryLinks");

            await table.DeleteIfExistsAsync();

            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = Model.Create<List<CategoryLinkChange>>()
                .SetEach(x => x.ChangeType = CategoryLinkChangeType.Add);

            var sut = new CategoryLinkStore(Config.Storage);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            var actual = (await sut.GetCategoryLinks(Group, categoryName, CancellationToken.None).ConfigureAwait(false))
                .ToList();

            actual.All(x => x.CategoryGroup == Group).Should().BeTrue();
            actual.All(x => x.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            actual.ShouldAllBeEquivalentTo(changes, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task StoreCategoryLinksCanAddAndRemoveAllItemsTest()
        {
            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = Model.Create<List<CategoryLinkChange>>()
                .SetEach(x => x.ChangeType = CategoryLinkChangeType.Add);

            var sut = new CategoryLinkStore(Config.Storage);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            changes.SetEach(x => x.ChangeType = CategoryLinkChangeType.Remove);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetCategoryLinks(Group, categoryName, CancellationToken.None).ConfigureAwait(false);

            actual.Should().BeEmpty();
        }

        [Fact]
        public void StoreCategoryLinksIgnoresDeletingItemsNotFoundTest()
        {
            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = new List<CategoryLinkChange>
            {
                new CategoryLinkChange
                {
                    ChangeType = CategoryLinkChangeType.Remove,
                    ProfileId = Guid.NewGuid()
                }
            };

            var sut = new CategoryLinkStore(Config.Storage);

            Func<Task> action = async () => await sut
                .StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            action.ShouldNotThrow();
        }

        [Fact]
        public void StoreCategoryLinksIgnoresEmptyChangesTest()
        {
            var categoryName = Guid.NewGuid().ToString();
            var changes = new List<CategoryLinkChange>();

            var sut = new CategoryLinkStore(Config.Storage);

            Func<Task> action = async () => await sut
                .StoreCategoryLinks(CategoryGroup.Gender, categoryName, changes, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldNotThrow();
        }

        [Fact]
        public async Task StoreCategoryLinksOverwritesExistingEntryUsingNewBatchTest()
        {
            const CategoryGroup Group = CategoryGroup.Gender;
            var categoryName = Guid.NewGuid().ToString();
            var changes = new List<CategoryLinkChange>
            {
                new CategoryLinkChange
                {
                    ChangeType = CategoryLinkChangeType.Add,
                    ProfileId = Guid.NewGuid()
                }
            };

            var sut = new CategoryLinkStore(Config.Storage);

            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);
            await sut.StoreCategoryLinks(Group, categoryName, changes, CancellationToken.None).ConfigureAwait(false);

            var actual = (await sut.GetCategoryLinks(Group, categoryName, CancellationToken.None).ConfigureAwait(false))
                .ToList();

            actual.All(x => x.CategoryGroup == Group).Should().BeTrue();
            actual.All(x => x.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            actual.ShouldAllBeEquivalentTo(changes, opt => opt.ExcludingMissingMembers());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void StoreCategoryLinksThrowsExceptionWithInvalidCategoryNameTest(string categoryName)
        {
            var changes = new List<CategoryLinkChange>();

            var sut = new CategoryLinkStore(Config.Storage);

            Func<Task> action = async () => await sut
                .StoreCategoryLinks(CategoryGroup.Gender, categoryName, changes, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void StoreCategoryLinksThrowsExceptionWithNullChangesTest()
        {
            var categoryName = Guid.NewGuid().ToString();

            var sut = new CategoryLinkStore(Config.Storage);

            Func<Task> action = async () => await sut
                .StoreCategoryLinks(CategoryGroup.Gender, categoryName, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigurationTest()
        {
            Action action = () => new AccountStore(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}