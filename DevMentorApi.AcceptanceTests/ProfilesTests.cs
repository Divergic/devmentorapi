namespace DevMentorApi.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Model;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class ProfilesTests
    {
        private readonly ILogger<ProfilesTests> _logger;
        private readonly ITestOutputHelper _output;

        public ProfilesTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<ProfilesTests>();
        }

        [Theory]
        [InlineData(ProfileStatus.Hidden, false)]
        [InlineData(ProfileStatus.Available, true)]
        [InlineData(ProfileStatus.Unavailable, true)]
        public async Task ReturnsProfileBasedOnStatusTest(ProfileStatus status, bool found)
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Status = status).Save()
                .ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger).ConfigureAwait(false);

            if (found)
            {
                actual.Should().Contain(x => x.Id == profile.Id);
            }
            else
            {
                actual.Should().NotContain(x => x.Id == profile.Id);
            }
        }
    }
}