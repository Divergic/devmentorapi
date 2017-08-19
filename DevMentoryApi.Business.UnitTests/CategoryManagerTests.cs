namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class CategoryManagerTests
    {
        [Fact]
        public async Task CreateCategoryPreservesExistingLinkCountWhenCategoryAlreadyExistsTest()
        {
            var expected = Model.Create<NewCategory>();
            var category = Model.Create<Category>().Set(x => x.Group = expected.Group).Set(x => x.Name = expected.Name);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(category);

                await sut.CreateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received(1).StoreCategory(Arg.Any<Category>(), tokenSource.Token).ConfigureAwait(false);
                await store.Received().StoreCategory(
                    Arg.Is<Category>(x => x.Group == expected.Group),
                    tokenSource.Token).ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Name == expected.Name), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().StoreCategory(
                    Arg.Is<Category>(x => x.LinkCount == category.LinkCount),
                    tokenSource.Token).ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Reviewed), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Visible), tokenSource.Token)
                    .ConfigureAwait(false);

                cache.Received().RemoveCategories();
            }
        }

        [Fact]
        public async Task CreateCategoryProvidesCategoryToStoreTest()
        {
            var expected = Model.Create<NewCategory>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.CreateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received(1).StoreCategory(Arg.Any<Category>(), tokenSource.Token).ConfigureAwait(false);
                await store.Received().StoreCategory(
                    Arg.Is<Category>(x => x.Group == expected.Group),
                    tokenSource.Token).ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Name == expected.Name), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.LinkCount == 0), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Reviewed), tokenSource.Token)
                    .ConfigureAwait(false);
                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Visible), tokenSource.Token)
                    .ConfigureAwait(false);

                cache.Received().RemoveCategories();
            }
        }

        [Fact]
        public void CreateCategoryThrowsExceptionWithNullCategoryTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

            Func<Task> action = async () => await sut.CreateCategory(null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task GetCategoriesCachesCategoryReturnedFromStoreTest()
        {
            var expected = Model.Create<List<Category>>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreCategories(
                    Verify.That<ICollection<Category>>(x => x.ShouldAllBeEquivalentTo(expected)));
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsCachedCategoriesTest()
        {
            var expected = Model.Create<List<Category>>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            cache.GetCategories().Returns(expected);

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
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

                var sut = new CategoryManager(store, cache);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsEmptyListWhenCategoriesNotFoundTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

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

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsVisibleCategoriesFromStoreWhenVisibleOnlyRequestedTest()
        {
            var categories = Model.Create<List<Category>>();
            var expected = categories.Where(x => x.Visible);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryManager(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>)null);
                store.GetAllCategories(tokenSource.Token).Returns(categories);

                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<ICategoryStore>();

            Action action = () => new CategoryManager(store, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCategoryStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new CategoryManager(null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}