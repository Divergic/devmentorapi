namespace TechMentorApi.AcceptanceTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using TechMentorApi.AcceptanceTests.Properties;
    using TechMentorApi.Model;
    using Xunit;
    using Xunit.Abstractions;

    public class PhotoTests
    {
        private readonly ILogger<PhotoTests> _logger;
        private readonly ITestOutputHelper _output;

        public PhotoTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<PhotoTests>();
        }

        [Fact]
        public async Task GetReturnsStoredPhotoForAnonymousUserTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, expected, identity)
                .ConfigureAwait(false);

            var details = result.Item2;
            
            var location = ApiLocation.PhotoFor(details);

            var actual = await Client.Get<byte[]>(location, _logger).ConfigureAwait(false);

            actual.SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public async Task GetReturnsStoredPhotoForProfileIdentityTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, expected, identity)
                .ConfigureAwait(false);

            var details = result.Item2;
            
            var location = ApiLocation.PhotoFor(details);

            var actual = await Client.Get<byte[]>(location, _logger, identity).ConfigureAwait(false);

            actual.SequenceEqual(expected).Should().BeTrue();
        }

        [Fact]
        public async Task GetReturnsStoredPhotoWithoutHashQueryTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            var result = await Client.PostFile<PhotoDetails>(address, _logger, expected, identity)
                .ConfigureAwait(false);

            var details = result.Item2;

            details.Hash = null;

            var location = ApiLocation.PhotoFor(details);

            var actual = await Client.Get<byte[]>(location, _logger).ConfigureAwait(false);

            actual.SequenceEqual(expected).Should().BeTrue();
        }
    }
}