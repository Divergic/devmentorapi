namespace TechMentorApi.AcceptanceTests
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using SixLabors.ImageSharp;
    using TechMentorApi.AcceptanceTests.Properties;
    using TechMentorApi.Model;
    using Xunit;
    using Xunit.Abstractions;

    public class PhotosTests
    {
        private readonly ILogger<PhotosTests> _logger;
        private readonly ITestOutputHelper _output;

        public PhotosTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<PhotosTests>();
        }

        [Fact]
        public async Task PostResizedPhotoRetainingAspectRatioWhenTooLargeTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, Resources.aspect, identity, "image/png")
                .ConfigureAwait(false);

            var location = result.Item1;

            var actual = await Client.Get<byte[]>(location, _logger).ConfigureAwait(false);

            using (var image = Image.Load(actual))
            {
                image.Height.Should().Be(300);
                image.Width.Should().Be(200);
            }
        }

        [Fact]
        public async Task PostResizedPhotoWhenTooLargeTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, Resources.resize, identity)
                .ConfigureAwait(false);

            var location = result.Item1;

            var actual = await Client.Get<byte[]>(location, _logger).ConfigureAwait(false);

            using (var image = Image.Load(actual))
            {
                image.Height.Should().Be(300);
                image.Width.Should().Be(300);
            }
        }

        [Theory]
        [InlineData("application/octet-stream")]
        [InlineData("image/gif")]
        public async Task PostReturnsBadRequestForUnsupportedContentTypeTest(string contentType)
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;

            await Client.PostFile<PhotoDetails>(address, _logger, Resources.photo, identity,
                    contentType, HttpStatusCode.BadRequest)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenFileTooLargeTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;

            await Client.PostFile<PhotoDetails>(address, _logger, Resources.oversize, identity, "image/jpeg",
                    HttpStatusCode.BadRequest)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenNoContentProvidedTest()
        {
            var identity = ClaimsIdentityFactory.Build();
            var address = ApiLocation.AccountProfilePhotos;

            await Client
                .Post(address, _logger, null, identity, HttpStatusCode.BadRequest)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task PostReturnsCreatedForNewPhotoTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;

            var actual = await Client.PostFile<PhotoDetails>(address, _logger, Resources.photo, identity)
                .ConfigureAwait(false);

            var details = actual.Item2;

            details.Hash.Should().NotBeNullOrWhiteSpace();
            details.Id.Should().NotBeEmpty();
            details.ProfileId.Should().Be(profile.Id);
        }

        [Fact]
        public async Task PostReturnsLocationOfCreatedPhotoTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, expected, identity)
                .ConfigureAwait(false);

            var location = result.Item1;

            var actual = await Client.Get<byte[]>(location, _logger, identity).ConfigureAwait(false);

            actual.SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public async Task PostReturnsUnauthorizedForAnonymousUserTest()
        {
            var address = ApiLocation.AccountProfilePhotos;

            await Client.Post(address, _logger, null, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }
    }
}