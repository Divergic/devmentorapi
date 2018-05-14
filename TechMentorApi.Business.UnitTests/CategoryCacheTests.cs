namespace TechMentorApi.Business.UnitTests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Model;
    using Xunit;

    public class CategoryCacheTests
    {
        [Fact]
        public void GetCategoriesReturnsCachedCategoriesTest()
        {
            var expected = Model.Create<List<Category>>();
            const string CacheKey = "Categories";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategories();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCategoriesReturnsNullWhenCachedCategoriesNotFoundTest()
        {
            const string CacheKey = "Categories";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(CacheKey, out value).Returns(x => false);

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategories();

            actual.Should().BeNull();
        }

        [Fact]
        public void GetCategoryLinksReturnsCachedCategoryLinksTest()
        {
            var filter = Model.Create<ProfileFilter>();
            var expected = Model.Create<List<Guid>>();
            var cacheKey = "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategoryLinks(filter);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCategoryLinksReturnsNullWhenCachedCategoryLinksNotFoundTest()
        {
            var filter = Model.Create<ProfileFilter>();
            var cacheKey = "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(x => false);

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategoryLinks(filter);

            actual.Should().BeNull();
        }

        [Fact]
        public void GetCategoryLinksThrowsExceptionWithNullFilterTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.GetCategoryLinks(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetCategoryReturnsCachedCategoryTest()
        {
            var expected = Model.Create<Category>();
            var cacheKey = "Category|" + expected.Group + "|" + expected.Name;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(
                x =>
                {
                    x[1] = expected;

                    return true;
                });

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategory(expected.Group, expected.Name);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCategoryReturnsNullWhenCachedCategoryNotFoundTest()
        {
            var expected = Model.Create<Category>();
            var cacheKey = "Category|" + expected.Group + "|" + expected.Name;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            object value;

            cache.TryGetValue(cacheKey, out value).Returns(x => false);

            var sut = new CategoryCache(cache, config);

            var actual = sut.GetCategory(expected.Group, expected.Name);

            actual.Should().BeNull();
        }

        [Fact]
        public void RemoveCategoriesLinksRemovesFromCacheTest()
        {
            var filter = Model.Create<ProfileFilter>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheKey = "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;

            var sut = new CategoryCache(cache, config);

            sut.RemoveCategoryLinks(filter);

            cache.Received().Remove(cacheKey);
        }

        [Fact]
        public void RemoveCategoriesLinksThrowsExceptionWithNullFilterTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.RemoveCategoryLinks(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RemoveCategoriesRemovesFromCacheTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            sut.RemoveCategories();

            cache.Received().Remove("Categories");
        }
        
        [Fact]
        public void RemoveCategoryRemovesFromCacheTest()
        {
            var expected = Model.Create<Category>();
            var cacheKey = "Category|" + expected.Group + "|" + expected.Name;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            sut.RemoveCategory(expected.Group, expected.Name);

            cache.Received().Remove(cacheKey);
        }

        [Fact]
        public void StoreCategoriesThrowsExceptionWithNullCategoriesTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.StoreCategories(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreCategoriesWritesCategoriesToCacheTest()
        {
            var expected = Model.Create<List<Category>>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            const string CacheKey = "Categories";

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.CategoriesExpiration.Returns(cacheExpiry);
            cache.CreateEntry(CacheKey).Returns(cacheEntry);

            var sut = new CategoryCache(cache, config);

            sut.StoreCategories(expected);

            cacheEntry.Value.Should().Be(expected);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }

        [Fact]
        public void StoreCategoryLinksThrowsExceptionWithNullCategoryLinksTest()
        {
            var filter = Model.Create<ProfileFilter>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.StoreCategoryLinks(filter, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreCategoryLinksThrowsExceptionWithNullFilterTest()
        {
            var links = Model.Create<List<Guid>>();
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.StoreCategoryLinks(null, links);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreCategoryLinksWritesCategoryLinksToCacheTest()
        {
            var expected = Model.Create<List<Guid>>();
            var cacheExpiry = TimeSpan.FromMinutes(23);
            var filter = Model.Create<ProfileFilter>();
            var cacheKey = "CategoryLinks|" + filter.CategoryGroup + "|" + filter.CategoryName;

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.CategoryLinksExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new CategoryCache(cache, config);

            sut.StoreCategoryLinks(filter, expected);

            cacheEntry.Value.Should().Be(expected);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }

        [Fact]
        public void StoreCategoryThrowsExceptionWithNullCategoryTest()
        {
            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();

            var sut = new CategoryCache(cache, config);

            Action action = () => sut.StoreCategory(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StoreCategoryWritesCategoryToCacheTest()
        {
            var expected = Model.Create<Category>();
            var cacheKey = "Category|" + expected.Group + "|" + expected.Name;
            var cacheExpiry = TimeSpan.FromMinutes(23);

            var cache = Substitute.For<IMemoryCache>();
            var config = Substitute.For<ICacheConfig>();
            var cacheEntry = Substitute.For<ICacheEntry>();

            config.CategoriesExpiration.Returns(cacheExpiry);
            cache.CreateEntry(cacheKey).Returns(cacheEntry);

            var sut = new CategoryCache(cache, config);

            sut.StoreCategory(expected);

            cacheEntry.Value.Should().Be(expected);
            cacheEntry.SlidingExpiration.Should().Be(cacheExpiry);
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var config = Substitute.For<ICacheConfig>();

            Action action = () => new CategoryCache(null, config);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullConfigTest()
        {
            var cache = Substitute.For<IMemoryCache>();

            Action action = () => new CategoryCache(cache, null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}