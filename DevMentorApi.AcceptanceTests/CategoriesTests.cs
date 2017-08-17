namespace DevMentorApi.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using DevMentorApi.ViewModels;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class CategoriesTests
    {
        private readonly ILogger<CategoriesTests> _logger;
        private readonly ITestOutputHelper _output;

        public CategoriesTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<CategoriesTests>();
        }

        [Fact]
        public async Task GetReturnsAllCategoriesForAdministratorTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            var actual = await Client.Get<List<Category>>(address, _logger, administrator).ConfigureAwait(false);

            actual.Should().Contain(x => x.Group == expected.Group && x.Name == expected.Name);

            // TODO: Prove that hidden tasks are also returned to the administrator
        }

        [Fact]
        public async Task GetReturnsVisibleCategoriesForAnonymousUserTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var address = ApiLocation.Categories;

            var actual = await Client.Get<List<PublicCategory>>(address, _logger).ConfigureAwait(false);

            actual.Should().Contain(x => x.Group == expected.Group && x.Name == expected.Name);

            // TODO: Prove that hidden tasks are not returned to the user
        }

        [Fact]
        public async Task GetReturnsVisibleCategoriesForAuthenticatedUserTest()
        {
            var expected = await Model.Create<NewCategory>().Save().ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build();
            var address = ApiLocation.Categories;

            var actual = await Client.Get<List<PublicCategory>>(address, _logger, identity).ConfigureAwait(false);

            actual.Should().Contain(x => x.Group == expected.Group && x.Name == expected.Name);

            // TODO: Prove that hidden categories are not returned to the user
        }

        [Fact]
        public async Task PostAllowsCategoryToBeVisibleImmediatelyTest()
        {
            var expected = Model.Create<NewCategory>();
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            // Get the public categories to ensure they are cached in the API
            await Client.Get(ApiLocation.Categories, _logger, null).ConfigureAwait(false);

            await Client.Post(address, _logger, expected, identity).ConfigureAwait(false);

            // Get the public categories again for an anonymous user
            var categories = await Client.Get<List<PublicCategory>>(ApiLocation.Categories, _logger, null)
                .ConfigureAwait(false);

            var actual = categories.Single(x => x.Group == expected.Group && x.Name == expected.Name);

            actual.LinkCount.Should().Be(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PostReturnsBadRequestWhenNameNotProvidedTest(string name)
        {
            var model = Model.Create<NewCategory>().Set(x => x.Name = name);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, model, identity, HttpStatusCode.BadRequest).ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenNoContentProvidedTest()
        {
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, null, identity, HttpStatusCode.BadRequest).ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenUnsupportedGroupProvidedTest()
        {
            var model = Model.Create<NewCategory>().Set(x => x.Group = (CategoryGroup)1234);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, model, identity, HttpStatusCode.BadRequest).ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsCreatedForNewCategoryTest()
        {
            var model = Model.Create<NewCategory>();
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, model, identity).ConfigureAwait(false);
        }
        
        [Fact]
        public async Task PostReturnsForbiddenWhenUserNotAdministratorTest()
        {
            var identity = ClaimsIdentityFactory.Build();
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, null, identity, HttpStatusCode.Forbidden).ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsUnauthorizedForAnonymousUserTest()
        {
            var address = ApiLocation.Categories;

            await Client.Post(address, _logger, null, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }
    }
}