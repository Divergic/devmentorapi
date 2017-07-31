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
    using Microsoft.Extensions.Caching.Memory;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class CategoryManagerTests
    {
        [Fact]
        public async Task GetCategoriesCachesCategoryReturnedFromStoreTest()
        {
            var expected = Model.Create<List<Category>>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            const string CacheKey = "Categories";

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.CategoriesExpiration.Returns(cacheExpiry);
            cache.CreateEntry(CacheKey).Returns(cacheEntry);

            var sut = new CategoryManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                cacheEntry.Value.Should().Be(actual);
                cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsCachedCategoriesTest()
        {
            var expected = Model.Create<List<Category>>();
            const string CacheKey = "Categories";

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new CategoryManager(store, cache, config);

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
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAllCategories(tokenSource.Token).Returns(expected);

                var sut = new CategoryManager(store, cache, config);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsEmptyListWhenCategoriesNotFoundTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAllCategories(tokenSource.Token).Returns((IEnumerable<Category>)null);

                var actual = await sut.GetCategories(ReadType.All, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEmpty();
                cache.DidNotReceive().CreateEntry(Arg.Any<object>());
            }
        }

        [Fact]
        public async Task GetCategoriesReturnsVisibleCategoriesFromCacheWhenVisibleOnlyRequestedTest()
        {
            var categories = Model.Create<List<Category>>();
            var expected = categories.Where(x => x.Visible);
            const string CacheKey = "Categories";

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(
                x =>
                {
                    x[1] = categories;

                    return true;
                });

            var sut = new CategoryManager(store, cache, config);

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
            var cacheExpiry = TimeSpan.FromMinutes(23);
            const string CacheKey = "Categories";

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.CategoriesExpiration.Returns(cacheExpiry);
            cache.CreateEntry(CacheKey).Returns(cacheEntry);

            var sut = new CategoryManager(store, cache, config);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAllCategories(tokenSource.Token).Returns(categories);

                var actual = await sut.GetCategories(ReadType.VisibleOnly, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldAllBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new CategoryManager(store, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCategoryStoreTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new CategoryManager(null, cache, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<IMemoryCache>();

            Action action = () => new CategoryManager(store, cache, null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}