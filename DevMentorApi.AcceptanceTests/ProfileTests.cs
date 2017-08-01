namespace DevMentorApi.AcceptanceTests
{
    using System.Net;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
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
        public async Task GetForNewUserCreatesProfileAsUnavailableTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var identity = ClaimsIdentityFactory.Build(null, profile);
            var address = ApiLocation.Profile;

            var actual = await Client.Get<Profile>(address, _logger, identity).ConfigureAwait(false);

            actual.Status.Should().Be(ProfileStatus.Unavailable);
        }

        [Fact]
        public async Task GetForNewUserRegistersAccountAndReturnsNewProfileTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var identity = ClaimsIdentityFactory.Build(null, profile);
            var address = ApiLocation.Profile;

            var actual = await Client.Get<Profile>(address, _logger, identity).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.Excluding(x => x.AccountId).Excluding(x => x.Status));
        }

        [Theory]
        [InlineData("email", "first", "last")]
        [InlineData(null, "first", "last")]
        [InlineData("email", null, "last")]
        [InlineData("email", "first", null)]
        public async Task GetForNewUserRegistersAccountWithProvidedClaimsTest(
            string email,
            string firstName,
            string lastName)
        {
            var profile = new Profile
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
            var identity = ClaimsIdentityFactory.Build(null, profile);
            var address = ApiLocation.Profile;

            var actual = await Client.Get<Profile>(address, _logger, identity).ConfigureAwait(false);

            actual.ShouldBeEquivalentTo(profile, opt => opt.Excluding(x => x.AccountId));
        }

        [Fact]
        public async Task GetReturnsForbiddenForAnonymousUserTest()
        {
            var address = ApiLocation.Profile;

            await Client.Get(address, _logger, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }
    }
}