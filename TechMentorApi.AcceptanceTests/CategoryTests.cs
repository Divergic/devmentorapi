namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;
    using Xunit;
    using Xunit.Abstractions;

    public class CategoryTests
    {
        private readonly ILogger<CategoryTests> _logger;
        private readonly ITestOutputHelper _output;

        public CategoryTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<CategoryTests>();
        }

        [Fact]
        public async Task GetReturnsCategoryAfterApprovalForAdministratorTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();

            await Client.Get<PublicCategory>(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);

            await categoryApproval.Save(_logger, administrator).ConfigureAwait(false);

            var actual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newCategory, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsCategoryForAdministratorTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(expected);

            var actual = await Client.Get<Category>(address, _logger, administrator).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsCategoryWithCorrectLinkCountWhenProfileBannedTest()
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };

            await categoryApproval.Save(_logger).ConfigureAwait(false);

            var firstActual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            firstActual.LinkCount.Should().Be(1);

            await profile.Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            secondActual.LinkCount.Should().Be(0);
        }

        [Fact]
        public async Task GetReturnsCategoryWithCorrectLinkCountWhenProfileIsAddedToExistingCategoryTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };

            await categoryApproval.Save(_logger).ConfigureAwait(false);

            await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Skills.First().Name = newCategory.Name).Save().ConfigureAwait(false);

            var actual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            actual.LinkCount.Should().Be(2);
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public async Task GetReturnsCategoryWithCorrectLinkCountWhenProfileIsHiddenAndThenRestoredTest(
            ProfileStatus status)
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Status = status)
                .Save(_logger, account).ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };

            await categoryApproval.Save(_logger).ConfigureAwait(false);

            var firstActual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            firstActual.LinkCount.Should().Be(1);

            await profile.Set(x => x.Status = ProfileStatus.Hidden).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            secondActual.LinkCount.Should().Be(0);

            await profile.Set(x => x.Status = status).Save(_logger, account).ConfigureAwait(false);

            var thirdActual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            thirdActual.LinkCount.Should().Be(1);
        }

        [Fact]
        public async Task GetReturnsCategoryWithCorrectLinkCountWhenProfileIsRemovedFromExistingCategoryTest()
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };

            await categoryApproval.Save(_logger).ConfigureAwait(false);

            profile.Skills.Remove(profile.Skills.First());

            await profile.Save(_logger, account).ConfigureAwait(false);

            var actual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            actual.LinkCount.Should().Be(0);
        }

        [Fact]
        public async Task GetReturnsCategoryWithCorrectLinkCountWhenUsedAfterApprovalByAdministratorTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };

            await categoryApproval.Save(_logger).ConfigureAwait(false);

            var actual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newCategory, opt => opt.ExcludingMissingMembers());
            actual.LinkCount.Should().Be(1);
        }

        [Fact]
        public async Task GetReturnsNotFoundForInvalidCategoryGroupTest()
        {
            var address = ApiLocation.Category("some", "other");

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForUnapprovedCategoryForAnonymousUserTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);

            await Client.Get<PublicCategory>(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForUnapprovedCategoryForAuthenticatedUserTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var identity = ClaimsIdentityFactory.Build();

            await Client.Get<PublicCategory>(address, _logger, identity, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsUnapprovedCategoryForAdministratorTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Category(newCategory);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();

            var actual = await Client.Get<PublicCategory>(address, _logger, identity).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newCategory, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsVisibleCategoryForAnonymousUserTest()
        {
            var newCategory = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var address = ApiLocation.Category(newCategory);

            var actual = await Client.Get<PublicCategory>(address, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newCategory, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsVisibleCategoryForAuthenticatedUserTest()
        {
            var newCategory = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build();
            var address = ApiLocation.Category(newCategory);

            var actual = await Client.Get<PublicCategory>(address, _logger, identity).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(newCategory, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task PutMakesCategoryInvisibleImmediatelyTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(expected);
            var model = Model.Create<UpdateCategory>().Set(x => x.Visible = true);

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            var firstCategories = await Client.Get<List<PublicCategory>>(ApiLocation.Categories, _logger)
                .ConfigureAwait(false);

            // It is currently not visible so we should have it returned here
            var actual = firstCategories.Single(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.LinkCount.Should().Be(0);

            model.Visible = false;

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            // Get the public categories again for an anonymous user
            var secondCategories = await Client.Get<List<PublicCategory>>(ApiLocation.Categories, _logger)
                .ConfigureAwait(false);

            // It is currently not visible so we should have it returned here
            secondCategories.Should().NotContain(x => x.Group == expected.Group && x.Name == expected.Name);
        }

        [Fact]
        public async Task PutMakesCategoryVisibleImmediatelyTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(expected);
            var model = Model.Create<UpdateCategory>().Set(x => x.Visible = false);

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            // Get the public categories again for an anonymous user
            var firstCategories = await Client.Get<List<PublicCategory>>(ApiLocation.Categories, _logger)
                .ConfigureAwait(false);

            // It is currently not visible so we should have it returned here
            firstCategories.Should().NotContain(x => x.Group == expected.Group && x.Name == expected.Name);

            model.Visible = true;

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            var secondCategories = await Client.Get<List<PublicCategory>>(ApiLocation.Categories, _logger)
                .ConfigureAwait(false);

            // It is currently not visible so we should have it returned here
            var actual = secondCategories.Single(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.LinkCount.Should().Be(0);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenNoContentProvidedTest()
        {
            var model = Model.Create<Category>();
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(model);

            await Client.Put(address, _logger, null, identity, HttpStatusCode.BadRequest).ConfigureAwait(false);
        }

        [Fact]
        public async Task PutReturnsForbiddenWhenUserNotAdministratorTest()
        {
            var expected = Model.Create<Category>();
            var identity = ClaimsIdentityFactory.Build();
            var address = ApiLocation.Category(expected);

            await Client.Put(address, _logger, null, identity, HttpStatusCode.Forbidden).ConfigureAwait(false);
        }

        [Fact]
        public async Task PutReturnsNotFoundWhenCategoryDoesNotExistTest()
        {
            var expected = Model.Create<Category>();
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(expected);
            var model = Model.Create<UpdateCategory>().Set(x => x.Visible = true);

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task PutReturnsNotFoundWhenGroupIsInvalidTest()
        {
            var model = Model.Create<Category>().Set(x => x.Group = (CategoryGroup)int.MaxValue);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(model);

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task PutReturnsNotFoundWhenNameNotProvidedTest(string name)
        {
            var model = Model.Create<Category>().Set(x => x.Name = name);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Category(model);

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task PutReturnsUnauthorizedForAnonymousUserTest()
        {
            var expected = Model.Create<Category>();
            var address = ApiLocation.Category(expected);

            await Client.Put(address, _logger, null, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }

        [Fact]
        public async Task PutSetsReviewedToTrueTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var skill = profile.Skills.First();
            var category = new Category
            {
                Group = CategoryGroup.Skill,
                Name = skill.Name
            };
            var address = ApiLocation.Category(category);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var model = Model.Create<UpdateCategory>();

            await Client.Put(address, _logger, model, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            var categories = await Client.Get<List<Category>>(ApiLocation.Categories, _logger, identity)
                .ConfigureAwait(false);

            var actual = categories.Single(x => x.Group == category.Group && x.Name == category.Name);

            actual.Reviewed.Should().BeTrue();
        }
    }
}