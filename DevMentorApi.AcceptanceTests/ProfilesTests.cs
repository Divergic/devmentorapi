namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
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

        [Fact]
        public async Task GetDoesNotReturnBannedProfileTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save().ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger).ConfigureAwait(false);

            actual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public async Task GetDoesNotReturnProfileWhenBannedTest(ProfileStatus status)
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Status = status)
                .Save(_logger, account).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            firstActual.Should().Contain(x => x.Id == profile.Id);

            await profile.Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public async Task GetDoesNotReturnProfileWhenUpdatedToHiddenTest(ProfileStatus status)
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Status = status)
                .Save(_logger, account).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            firstActual.Should().Contain(x => x.Id == profile.Id);

            await profile.Set(x => x.Status = ProfileStatus.Hidden).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Fact]
        public async Task GetIgnoresUnsupportedFiltersTest()
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            var address = new Uri(ApiLocation.Profiles + "?unknown=filter");
            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.ShouldAllBeEquivalentTo(firstActual);
        }

        [Fact]
        public async Task GetReturnsMostRecentDataWhenProfileUpdatedTest()
        {
            var account = Model.Create<Account>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            var template = Model.Using<ProfileBuildStrategy>().Create<ProfileResult>();

            profile.BirthYear = template.BirthYear;
            profile.YearStartedInTech = template.YearStartedInTech;
            profile.FirstName = template.FirstName;
            profile.Gender = template.Gender;
            profile.LastName = template.LastName;
            profile.TimeZone = template.TimeZone;

            await profile.Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Theory]
        [InlineData(ProfileStatus.Hidden, false)]
        [InlineData(ProfileStatus.Available, true)]
        [InlineData(ProfileStatus.Unavailable, true)]
        public async Task GetReturnsProfileBasedOnStatusTest(ProfileStatus status, bool found)
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