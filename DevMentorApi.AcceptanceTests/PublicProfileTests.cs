namespace DevMentorApi.AcceptanceTests
{
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using DevMentorApi.ViewModels;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
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
        public async Task GetReturnsOkForAnonymousUserTest()
        {
            var profile = await Model.Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.PublicProfileFor(profile.AccountId);

            var actual = await Client.Get<PublicProfile>(address, null, null, _logger).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }
    }
}