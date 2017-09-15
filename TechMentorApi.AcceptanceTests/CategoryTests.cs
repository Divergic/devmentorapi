namespace TechMentorApi.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Model;
    using ModelBuilder;
    using ViewModels;
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
            var model = Model.Create<Category>().Set(x => x.Group = (CategoryGroup) int.MaxValue);
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