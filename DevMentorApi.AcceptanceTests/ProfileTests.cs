namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Model;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class ProfileTests
    {
        private readonly ILogger<ProfileTests> _logger;
        private readonly ITestOutputHelper _output;

        public ProfileTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<ProfileTests>();
        }

        [Fact]
        public async Task GetReturnsNotFoundForBannedProfileTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save(_logger).ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForHiddenProfileTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Status = ProfileStatus.Hidden).Save(_logger).ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForInvalidIdTest()
        {
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }
    }
}