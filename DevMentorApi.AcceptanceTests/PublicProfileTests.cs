namespace DevMentorApi.AcceptanceTests
{
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Model;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class PublicProfileTests
    {
        private readonly ILogger<PublicProfileTests> _logger;
        private readonly ITestOutputHelper _output;

        public PublicProfileTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<PublicProfileTests>();
        }

        [Fact]
        public async Task GetReturnsNotFoundForHiddenProfileTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Status = ProfileStatus.Hidden).Save(_logger).ConfigureAwait(false);
            var address = ApiLocation.PublicProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForInvalidIdTest()
        {
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();
            var address = ApiLocation.PublicProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.PublicProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }
    }
}