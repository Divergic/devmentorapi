namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;
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
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var accountIdentity = ClaimsIdentityFactory.Build(account);
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
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
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
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
        public async Task DeleteReturnsNotFoundWhenProfileAlreadyBannedTest()
        {
            var account = Model.Create<Account>();
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Save(_logger, account)
                .ConfigureAwait(false);
            var newCategory = profile.Skills.First();
            var address = ApiLocation.Categories;
            var categoryApproval = new NewCategory
            {
                Group = CategoryGroup.Skill,
                Name = newCategory.Name
            };
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var profileAddress = ApiLocation.ProfileFor(profile.Id);

            await Client.Post(address, _logger, categoryApproval, administrator).ConfigureAwait(false);

            var firstActual = await Client.Get<List<PublicCategory>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Group == CategoryGroup.Skill && x.Name == newCategory.Name).LinkCount.Should()
                .Be(1);

            await Client.Delete(profileAddress, _logger, administrator).ConfigureAwait(false);

            var secondActual = await Client.Get<List<PublicCategory>>(address, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Group == CategoryGroup.Skill && x.Name == newCategory.Name).LinkCount.Should()
                .Be(0);

            await Client.Delete(profileAddress, _logger, administrator, HttpStatusCode.NotFound).ConfigureAwait(false);

            var thirdActual = await Client.Get<List<PublicCategory>>(address, _logger).ConfigureAwait(false);

            thirdActual.Single(x => x.Group == CategoryGroup.Skill && x.Name == newCategory.Name).LinkCount.Should()
                .Be(0);
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
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Delete(address, _logger, null, HttpStatusCode.Unauthorized).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetDoesNotReturnProfileEmailTest()
        {
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Save().ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<Profile>(address, _logger).ConfigureAwait(false);

            actual.Email.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetOnlyIncludesApprovedGenderTest(bool approved)
        {
            var categoryName = Guid.NewGuid().ToString();

            if (approved)
            {
                // Store the category as an administrator which will also make it approved by default
                var category = new NewCategory
                {
                    Group = CategoryGroup.Gender,
                    Name = categoryName
                };

                await category.Save(_logger).ConfigureAwait(false);
            }

            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Gender = categoryName)
                .Save(_logger).ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            if (approved)
            {
                actual.Gender.Should().Be(profile.Gender);
            }
            else
            {
                actual.Gender.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetOnlyIncludesApprovedLanguageTest(bool approved)
        {
            var categoryName = Guid.NewGuid().ToString();

            if (approved)
            {
                // Store the category as an administrator which will also make it approved by default
                var category = new NewCategory
                {
                    Group = CategoryGroup.Language,
                    Name = categoryName
                };

                await category.Save(_logger).ConfigureAwait(false);
            }

            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Languages.Add(categoryName))
                .Save(_logger).ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            if (approved)
            {
                actual.Languages.Should().Contain(x => x == categoryName);
            }
            else
            {
                actual.Languages.Should().NotContain(x => x == categoryName);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetOnlyIncludesApprovedSkillTest(bool approved)
        {
            var categoryName = Guid.NewGuid().ToString();

            if (approved)
            {
                // Store the category as an administrator which will also make it approved by default
                var category = new NewCategory
                {
                    Group = CategoryGroup.Skill,
                    Name = categoryName
                };

                await category.Save(_logger).ConfigureAwait(false);
            }

            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Skills.First().Name = categoryName)
                .Save(_logger).ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            if (approved)
            {
                actual.Skills.Should().Contain(x => x.Name == categoryName);
            }
            else
            {
                actual.Skills.Should().NotContain(x => x.Name == categoryName);
            }
        }

        [Fact]
        public async Task GetReturnsCategoryCorrectlyWhenToggledBetweenVisibleAndInvisibleTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories()
                .Set(x => x.Gender = Guid.NewGuid().ToString());

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var firstActual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            firstActual.Gender.Should().Be(profile.Gender);

            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var categoryAddress = ApiLocation.Category(CategoryGroup.Gender, profile.Gender);
            var updateCategory = new UpdateCategory
                {Visible = false};

            // Hide the gender category
            await Client.Put(categoryAddress, _logger, updateCategory, administrator, HttpStatusCode.NoContent)
                .ConfigureAwait(false);

            var secondActual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            secondActual.Gender.Should().BeNullOrEmpty();

            updateCategory.Visible = true;

            // Show the gender category again
            await Client.Put(categoryAddress, _logger, updateCategory, administrator, HttpStatusCode.NoContent)
                .ConfigureAwait(false);

            var thirdActual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            thirdActual.Gender.Should().Be(profile.Gender);
        }

        [Fact]
        public async Task GetReturnsNotFoundForBannedProfileTest()
        {
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save(_logger).ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForHiddenProfileTest()
        {
            var profile = await Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Status = ProfileStatus.Hidden).Save(_logger).ConfigureAwait(false);
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsNotFoundForInvalidIdTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>();
            var address = ApiLocation.ProfileFor(profile.Id);

            await Client.Get(address, _logger, null, HttpStatusCode.NotFound).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetReturnsSkillAfterApprovedForAnonymousUserTest()
        {
            var account = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Account>();
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().ClearCategories();
            
            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var newSkill = new Skill
            {
                Level = SkillLevel.Expert,
                Name = Guid.NewGuid().ToString("N")
            };

            profile.Skills.Add(newSkill);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var profileAddress = ApiLocation.ProfileFor(profile.Id);

            var firstActual = await Client.Get<PublicProfile>(profileAddress, _logger).ConfigureAwait(false);

            firstActual.Skills.Should().NotContain(x => x.Name == newSkill.Name);

            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();

            await new NewCategory { Group = CategoryGroup.Skill, Name = newSkill.Name }.Save(_logger, administrator).ConfigureAwait(false);

            var secondActual = await Client.Get<PublicProfile>(profileAddress, _logger).ConfigureAwait(false);

            secondActual.Skills.Should().Contain(x => x.Name == newSkill.Name);
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>();

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserWhenGenderIsNullTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Gender = null);

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserWhenLanguagesIsNullTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Languages = null);

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsOkForAnonymousUserWhenSkillsIsNullTest()
        {
            var profile = Model.UsingBuildStrategy<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Skills = null);

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var address = ApiLocation.ProfileFor(profile.Id);

            var actual = await Client.Get<PublicProfile>(address, _logger).ConfigureAwait(false);

            actual.Should().BeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }
    }
}