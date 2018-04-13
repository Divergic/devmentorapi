namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using ModelBuilder;
    using Newtonsoft.Json;
    using TechMentorApi.AcceptanceTests.Properties;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;
    using Xunit;
    using Xunit.Abstractions;

    public class ExportTests
    {
        private readonly ILogger<ExportTests> _logger;
        private readonly ITestOutputHelper _output;

        public ExportTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<ExportTests>();
        }

        [Fact]
        public async Task GetReturnsBannedProfileTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow).ClearCategories().Save(_logger, account)
                .ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfileExport;

            var actual = await Client.Get<ExportProfile>(address, _logger, identity).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.Excluding(x => x.Id).Excluding(x => x.BannedAt));
            actual.BannedAt.Should().BeCloseTo(profile.BannedAt.Value, 20000);
        }

        [Fact]
        public async Task GetReturnsExistingProfileTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfileExport;

            var actual = await Client.Get<ExportProfile>(address, _logger, identity).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.Excluding(x => x.Id));
        }

        [Fact]
        public async Task GetReturnsForbiddenForAnonymousUserTest()
        {
            var address = ApiLocation.AccountProfileExport;

            await Client.Get(address, _logger, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsHiddenProfileTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Status = ProfileStatus.Hidden).Save(_logger, account)
                .ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var address = ApiLocation.AccountProfileExport;

            var actual = await Client.Get<ExportProfile>(address, _logger, identity).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.Excluding(x => x.Id));
        }

        [Fact]
        public async Task GetReturnsProfileWithMultiplePhotosTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var profileAddress = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            await Client.PostFile<PhotoDetails>(profileAddress, _logger, expected, identity)
                .ConfigureAwait(false);
            await Client.PostFile<PhotoDetails>(profileAddress, _logger, expected, identity)
            .ConfigureAwait(false);
            var result = await Client.PostFile<PhotoDetails>(profileAddress, _logger, expected, identity)
                .ConfigureAwait(false);

            var photo = result.Item2;

            var export = await Client.Get<ExportProfile>(ApiLocation.AccountProfileExport, _logger, identity).ConfigureAwait(false);

            export.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
            export.Photos.Should().HaveCount(3);
            export.Photos.All(x => x.Hash == photo.Hash).Should().BeTrue();
            export.Photos.All(x => x.ProfileId == profile.Id).Should().BeTrue();
            export.Photos.All(x => x.Data.SequenceEqual(expected)).Should().BeTrue();
        }

        [Fact]
        public async Task GetReturnsProfileWithSinglePhotoTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Save(_logger, account).ConfigureAwait(false);
            var identity = ClaimsIdentityFactory.Build(account, profile);
            var profileAddress = ApiLocation.AccountProfilePhotos;
            var expected = Resources.photo;

            var result = await Client.PostFile<PhotoDetails>(profileAddress, _logger, expected, identity)
                .ConfigureAwait(false);

            var photo = result.Item2;

            var export = await Client.Get<ExportProfile>(ApiLocation.AccountProfileExport, _logger, identity).ConfigureAwait(false);

            export.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
            export.Photos.Should().HaveCount(1);
            export.Photos[0].Should().BeEquivalentTo(photo, opt => opt.ExcludingMissingMembers());
            export.Photos[0].Data.SequenceEqual(expected).Should().BeTrue();
        }
    }
}