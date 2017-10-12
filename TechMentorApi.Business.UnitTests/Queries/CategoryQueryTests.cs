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
                cache.GetCategories().Returns((ICollection<Category>) null);
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

            var sut = new CategoryQuery(store, cache);

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
                cache.GetCategories().Returns((ICollection<Category>) null);
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                var sut = new CategoryQuery(store, cache);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
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
                cache.GetCategories().Returns((ICollection<Category>) null);
                store.GetAllCategories(tokenSource.Token).Returns((IEnumerable<Category>) null);

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

            var sut = new CategoryQuery(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetCategories().Returns((ICollection<Category>) null);
                store.GetAllCategories(tokenSource.Token).Returns(categories);

                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<ICategoryStore>();

            Action action = () => new CategoryQuery(store, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCategoryStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new CategoryQuery(null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}