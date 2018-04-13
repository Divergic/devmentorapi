namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;

    public class CategoryCommandTests
    {
        [Fact]
        public async Task CreateCategoryPreservesExistingLinkCountWhenCategoryAlreadyExistsTest()
        {
            var expected = Model.Create<NewCategory>();
            var category = Model.Create<Category>().Set(x => x.Group = expected.Group).Set(x => x.Name = expected.Name);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

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

            var sut = new CategoryCommand(store, cache);

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
                cache.Received().RemoveCategory(expected.Group, expected.Name);
            }
        }

        [Fact]
        public async Task CreateCategorySetsLinkCountToZeroWhenNoExistingCategoryTest()
        {
            var expected = Model.Create<NewCategory>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.CreateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received().StoreCategory(Arg.Is<Category>(x => x.LinkCount == 0), tokenSource.Token)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public void CreateCategoryThrowsExceptionWithNullCategoryTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            Func<Task> action = async () => await sut.CreateCategory(null, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<ICategoryStore>();

            Action action = () => new CategoryCommand(store, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCategoryStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new CategoryCommand(null, cache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateCategoryPreservesExistingLinkCountWhenCategoryAlreadyExistsTest()
        {
            var expected = Model.Create<Category>();
            var category = Model.Create<Category>().Set(x => x.Group = expected.Group).Set(x => x.Name = expected.Name);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(category);

                await sut.UpdateCategory(expected, tokenSource.Token).ConfigureAwait(false);

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
                await store.Received()
                    .StoreCategory(Arg.Is<Category>(x => x.Visible == expected.Visible), tokenSource.Token)
                    .ConfigureAwait(false);

                cache.Received().RemoveCategories();
            }
        }

        [Fact]
        public async Task UpdateCategoryProvidesCategoryToStoreTest()
        {
            var expected = Model.Create<Category>();
            var category = Model.Create<Category>().Set(x => x.Group = expected.Group).Set(x => x.Name = expected.Name);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(category);

                await sut.UpdateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received().StoreCategory(expected, tokenSource.Token).ConfigureAwait(false);

                cache.Received().RemoveCategories();
                cache.Received().RemoveCategory(expected.Group, expected.Name);
            }
        }

        [Fact]
        public async Task UpdateCategorySetsReviewedToTrueTest()
        {
            var expected = Model.Create<Category>().Set(x => x.Reviewed = false);
            var category = Model.Create<Category>().Set(x => x.Group = expected.Group).Set(x => x.Name = expected.Name);

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetCategory(expected.Group, expected.Name, tokenSource.Token).Returns(category);

                await sut.UpdateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received().StoreCategory(Arg.Is<Category>(x => x.Reviewed), tokenSource.Token)
                    .ConfigureAwait(false);
            }
        }

        [Fact]
        public void UpdateCategoryThrowsExceptionWhenCategoryNotFoundTest()
        {
            var expected = Model.Create<Category>();

            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                Func<Task> action = async () =>
                    await sut.UpdateCategory(expected, tokenSource.Token).ConfigureAwait(false);

                action.Should().Throw<NotFoundException>();
            }
        }

        [Fact]
        public void UpdateCategoryThrowsExceptionWithNullCategoryTest()
        {
            var store = Substitute.For<ICategoryStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new CategoryCommand(store, cache);

            Func<Task> action = async () =>
                await sut.UpdateCategory(null, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}