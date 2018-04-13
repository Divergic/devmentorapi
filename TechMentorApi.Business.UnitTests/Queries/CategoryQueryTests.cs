namespace TechMentorApi.Business.UnitTests.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;
    using Xunit;

    public class CategoryQueryTests
    {
        [Fact]
        public async Task GetCategoriesCachesCategoryReturnedFromStoreTest()
        {
            var expected = Model.Create<List<Category>>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreCategories(
                    Verify.That<ICollection<Category>>(x => x.Should().BeEquivalentTo(expected)));
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsCachedCategoriesTest()
        {
            var expected = Model.Create<List<Category>>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            cache.GetCategories().Returns(expected);

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsCategoriesFromStoreWhenNotInCacheTest()
        {
            var expected = Model.Create<List<Category>>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                var sut = new CategoryQuery(store, cache);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsEmptyListWhenCategoriesNotFoundTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns((IEnumerable<Category>)null);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsVisibleCategoriesFromCacheWhenVisibleOnlyRequestedTest()
        {
            var categories = Model.Create<List<Category>>();
            var expected = categories.Where(x => x.Visible).ToList();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            cache.GetCategories().Returns(expected);

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsVisibleCategoriesFromStoreWhenVisibleOnlyRequestedTest()
        {
            var categories = Model.Create<List<Category>>();
            var expected = categories.Where(x => x.Visible);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns(categories);

                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoryDoesNotCacheNullValueFromStoreTest()
        {
            var expected = Model.Create<Category>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns((Category)null);

                var actual = await sut.GetCategory(ReadType.All, expected.Group, expected.Name, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreCategory(Arg.Any<Category>());
            }
        }

        [Theory]
        [InlineData(ReadType.All, true, true)]
        [InlineData(ReadType.All, false, true)]
        [InlineData(ReadType.VisibleOnly, true, true)]
        [InlineData(ReadType.VisibleOnly, false, false)]
        public async Task GetCategoryReturnsCategoryBasedOnReadTypeAndVisibilityTest(
            ReadType readType,
            bool isVisible,
            bool valueReturned)
        {
            var expected = Model.Create<Category>().Set(x => x.Visible = isVisible);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(expected);

                var actual = await sut.GetCategory(readType, expected.Group, expected.Name, tokenSource.Token)
                    .ConfigureAwait(false);

                if (valueReturned)
                {
                    actual.Should().BeEquivalentTo(expected);
                }
                else
                {
                    actual.Should().BeNull();
                }
            }
        }

        [Fact]
        public async Task GetCategoryReturnsCategoryFromCacheTest()
        {
            var expected = Model.Create<Category>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(expected);

                var actual = await sut.GetCategory(ReadType.All, expected.Group, expected.Name, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
                cache.Received().StoreCategory(expected);
            }
        }

        [Fact]
        public async Task GetCategoryStoresValueInCacheWhenReturnedFromStoreTest()
        {
            var expected = Model.Create<Category>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategory(expected.Group, expected.Name).Returns(expected);

                var actual = await sut.GetCategory(ReadType.All, expected.Group, expected.Name, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
                await store.DidNotReceive().GetCategory(
                    Arg.Any<CategoryGroup>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetCategoryThrowsExceptionWithInvalidNameTest(string name)
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryQuery(store, cache);

            Func<Task> actual = async () =>
                await sut.GetCategory(ReadType.VisibleOnly, CategoryGroup.Skill, name, CancellationToken.None)
                    .ConfigureAwait(false);

            actual.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<ICategoryStore>();

            Action action = () => new CategoryQuery(store, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCategoryStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new CategoryQuery(null, cache);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}