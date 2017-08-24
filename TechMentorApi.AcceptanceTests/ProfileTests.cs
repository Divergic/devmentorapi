namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using TechMentorApi.Model;
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
        public async Task DeleteBansAccountTest()
        {
            var account = Model.Using<ProfileBuildStrategy>().Create<Account>();
            var accountIdentity = ClaimsIdentityFactory.Build(account);
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);
            var identity = ClaimsIdentityFactory.Build().AsAdministrator();

            await Client.Delete(address, _logger, identity).ConfigureAwait(false);

            var actual = await Client.Get<Profile>(ApiLocation.AccountProfile, _logger, accountIdentity)
                .ConfigureAwait(false);

            actual.BannedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, 5000);
        }

        [Fact]
        public async Task DeleteReturnsForbiddenWhenUserNotAdministratorTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);
            var identity = ClaimsIdentityFactory.Build();

            await Client.Delete(address, _logger, identity, HttpStatusCode.Forbidden).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteReturnsNotFoundForEmptyIdTest()
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var profileUri = ApiLocation.ProfileFor(Guid.Empty);

            await Client.Delete(profileUri, _logger, administrator, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteReturnsNotFoundWhenProfileDoesNotExistTest()
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var profileUri = ApiLocation.ProfileFor(Guid.NewGuid());

            await Client.Delete(profileUri, _logger, administrator, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteReturnsUnauthorizedForAnonymousUserTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Delete(address, _logger, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetDoesNotReturnProfileEmailTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<Profile>(address, _logger).ConfigureAwait(false);

            actual.Email.Should().BeNull();
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