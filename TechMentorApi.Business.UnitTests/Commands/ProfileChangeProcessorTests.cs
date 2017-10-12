namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfileChangeProcessorTests
    {
        [Fact]
        public async Task ExecuteAddsCategoryLinkForNewCategoryTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.CategoryChanges.Clear());
            var change = Model.Create<CategoryChange>().Set(x => x.ChangeType = CategoryLinkChangeType.Add);
            var categories = Model.Create<List<Category>>();

            changes.CategoryChanges.Add(change);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                categoryStore.GetAllCategories(tokenSource.Token).Returns(categories);

                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await linkStore.Received().StoreCategoryLink(
                    Arg.Is<CategoryGroup>(x => x == change.CategoryGroup),
                    Arg.Is<string>(x => x == change.CategoryName),
                    Arg.Is<CategoryLinkChange>(
                        x => x.ChangeType == CategoryLinkChangeType.Add && x.ProfileId == profile.Id),
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData("Female")]
        [InlineData("female")]
        [InlineData("FEMALE")]
        public async Task ExecuteAddsLinkToExistingCategoryUsingCaseInsensitiveNameMatchTest(string name)
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.CategoryChanges.Clear());
            var change = new CategoryChange
            {
                CategoryGroup = CategoryGroup.Gender,
                CategoryName = name,
                ChangeType = CategoryLinkChangeType.Add
            };
            var category = Model.Create<Category>().Set(x => x.Name = "Female")
                .Set(x => x.Group = CategoryGroup.Gender);
            var categories = Model.Create<List<Category>>();
            var previousLinkCount = category.LinkCount;

            changes.CategoryChanges.Add(change);
            categories.Add(category);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                categoryStore.GetAllCategories(tokenSource.Token).Returns(categories);

                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await categoryStore.Received().StoreCategory(category, tokenSource.Token).ConfigureAwait(false);
                await categoryStore.Received().StoreCategory(
                    Arg.Is<Category>(x => x.LinkCount == previousLinkCount + 1),
                    tokenSource.Token).ConfigureAwait(false);

                cacheManager.Received(1).StoreCategories(
                    Verify.That<ICollection<Category>>(
                        x => x.Should().Contain(
                            y => y.Group == category.Group && y.Name == category.Name &&
                                 y.LinkCount == previousLinkCount + 1)));
                categories.Should().Contain(
                    x => x.Group == category.Group && x.Name == category.Name && x.LinkCount == previousLinkCount + 1);

                await linkStore.Received().StoreCategoryLink(
                    Arg.Is<CategoryGroup>(x => x == category.Group),
                    Arg.Is<string>(x => x == category.Name),
                    Arg.Is<CategoryLinkChange>(
                        x => x.ChangeType == CategoryLinkChangeType.Add && x.ProfileId == profile.Id),
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ExecuteAddsNewCategoryWhenNotFoundInStoreTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.CategoryChanges.Clear());
            var change = Model.Create<CategoryChange>().Set(x => x.ChangeType = CategoryLinkChangeType.Add);
            var categories = Model.Create<List<Category>>();

            changes.CategoryChanges.Add(change);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                categoryStore.GetAllCategories(tokenSource.Token).Returns(categories);

                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await categoryStore.Received(1).StoreCategory(Arg.Any<Category>(), Arg.Any<CancellationToken>())
                    .ConfigureAwait(false);
                await categoryStore.Received().StoreCategory(
                    Arg.Is<Category>(x => x.Group == change.CategoryGroup),
                    tokenSource.Token).ConfigureAwait(false);
                await categoryStore.Received()
                    .StoreCategory(Arg.Is<Category>(x => x.Name == change.CategoryName), tokenSource.Token)
                    .ConfigureAwait(false);
                await categoryStore.Received().StoreCategory(Arg.Is<Category>(x => x.LinkCount == 1), tokenSource.Token)
                    .ConfigureAwait(false);
                await categoryStore.Received()
                    .StoreCategory(Arg.Is<Category>(x => x.Reviewed == false), tokenSource.Token).ConfigureAwait(false);
                await categoryStore.Received()
                    .StoreCategory(Arg.Is<Category>(x => x.Visible == false), tokenSource.Token).ConfigureAwait(false);

                cacheManager.Received(1).StoreCategories(
                    Verify.That<ICollection<Category>>(
                        x => x.Should().Contain(
                            y => y.Group == change.CategoryGroup && y.Name == change.CategoryName)));
                categories.Should().Contain(x => x.Group == change.CategoryGroup && x.Name == change.CategoryName);
            }
        }

        [Fact]
        public async Task ExecuteDoesNotApplyChangesWhenProfileAndCategoriesNotChangedTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = false)
                .Set(x => x.CategoryChanges.Clear());

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await profileStore.DidNotReceive().StoreProfile(Arg.Any<Profile>(), Arg.Any<CancellationToken>())
                    .ConfigureAwait(false);
                cacheManager.DidNotReceive().StoreProfile(Arg.Any<Profile>());
                await linkStore.DidNotReceive().StoreCategoryLink(
                    Arg.Any<CategoryGroup>(),
                    Arg.Any<string>(),
                    Arg.Any<CategoryLinkChange>(),
                    Arg.Any<CancellationToken>());
                await categoryStore.DidNotReceive().StoreCategory(Arg.Any<Category>(), Arg.Any<CancellationToken>());
            }
        }

        [Fact]
        public async Task ExecuteDoesNotUpdateProfileWhenNotChangedTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = false);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await profileStore.DidNotReceive().StoreProfile(Arg.Any<Profile>(), Arg.Any<CancellationToken>())
                    .ConfigureAwait(false);
                cacheManager.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task ExecuteProcessesMultipleCategoryLinksTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>();
            var categories = Model.Create<List<Category>>();

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                categoryStore.GetAllCategories(tokenSource.Token).Returns(categories);

                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await categoryStore.Received(changes.CategoryChanges.Count)
                    .StoreCategory(Arg.Any<Category>(), tokenSource.Token).ConfigureAwait(false);

                cacheManager.Received(1).StoreCategories(Arg.Any<ICollection<Category>>());

                await linkStore.Received(changes.CategoryChanges.Count).StoreCategoryLink(
                    Arg.Any<CategoryGroup>(),
                    Arg.Any<string>(),
                    Arg.Any<CategoryLinkChange>(),
                    tokenSource.Token).ConfigureAwait(false);

                cacheManager.Received(changes.CategoryChanges.Count).RemoveCategoryLinks(Arg.Any<ProfileFilter>());
            }
        }

        [Theory]
        [InlineData("Female")]
        [InlineData("female")]
        [InlineData("FEMALE")]
        public async Task ExecuteRemovesLinkFromExistingCategoryUsingCaseInsensitiveNameMatchTest(string name)
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.CategoryChanges.Clear());
            var change = new CategoryChange
            {
                CategoryGroup = CategoryGroup.Gender,
                CategoryName = name,
                ChangeType = CategoryLinkChangeType.Remove
            };
            var category = Model.Create<Category>().Set(x => x.Name = "Female")
                .Set(x => x.Group = CategoryGroup.Gender);
            var categories = Model.Create<List<Category>>();
            var previousLinkCount = category.LinkCount;

            changes.CategoryChanges.Add(change);
            categories.Add(category);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                categoryStore.GetAllCategories(tokenSource.Token).Returns(categories);

                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await categoryStore.Received().StoreCategory(category, tokenSource.Token).ConfigureAwait(false);
                await categoryStore.Received().StoreCategory(
                    Arg.Is<Category>(x => x.LinkCount == previousLinkCount - 1),
                    tokenSource.Token).ConfigureAwait(false);

                cacheManager.Received(1).StoreCategories(
                    Verify.That<ICollection<Category>>(
                        x => x.Should().Contain(
                            y => y.Group == category.Group && y.Name == category.Name &&
                                 y.LinkCount == previousLinkCount - 1)));
                categories.Should().Contain(
                    x => x.Group == category.Group && x.Name == category.Name && x.LinkCount == previousLinkCount - 1);

                await linkStore.Received().StoreCategoryLink(
                    Arg.Is<CategoryGroup>(x => x == category.Group),
                    Arg.Is<string>(x => x == category.Name),
                    Arg.Is<CategoryLinkChange>(
                        x => x.ChangeType == CategoryLinkChangeType.Remove && x.ProfileId == profile.Id),
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void ExecuteThrowsExceptionWithNullChangesTest()
        {
            var profile = Model.Create<Profile>();

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            Func<Task> action = async () => await sut.Execute(profile, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ExecuteThrowsExceptionWithNullProfileTest()
        {
            var changes = Model.Create<ProfileChangeResult>();

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            Func<Task> action = async () => await sut.Execute(null, changes, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteUpdatesProfileWhenChangedTest()
        {
            var profile = Model.Create<Profile>();
            var changes = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var profileStore = Substitute.For<IProfileStore>();
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            var sut = new ProfileChangeProcessor(profileStore, categoryStore, linkStore, cacheManager);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.Execute(profile, changes, tokenSource.Token).ConfigureAwait(false);

                await profileStore.Received().StoreProfile(profile, tokenSource.Token).ConfigureAwait(false);
                cacheManager.Received().StoreProfile(profile);
            }
        }

        [Fact]
        public void ThrowsExceptionWithNullCategoryStoreTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            Action action = () => new ProfileChangeProcessor(profileStore, null, linkStore, cacheManager);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullLinkCacheManagerTest()
        {
            var categoryStore = Substitute.For<ICategoryStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();

            Action action = () => new ProfileChangeProcessor(profileStore, categoryStore, linkStore, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullLinkCategoryStoreTest()
        {
            var categoryStore = Substitute.For<ICategoryStore>();
            var profileStore = Substitute.For<IProfileStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            Action action = () => new ProfileChangeProcessor(profileStore, categoryStore, null, cacheManager);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullProfileStoreTest()
        {
            var categoryStore = Substitute.For<ICategoryStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cacheManager = Substitute.For<ICacheManager>();

            Action action = () => new ProfileChangeProcessor(null, categoryStore, linkStore, cacheManager);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}