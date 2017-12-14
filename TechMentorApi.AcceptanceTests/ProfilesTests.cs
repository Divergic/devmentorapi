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
        [InlineData(null)]
        [InlineData("Male")]
        public async Task GetDoesNotReturnProfileAfterGenderUpdatedTest(string newGender)
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Gender = "Female");

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = profile.Gender
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            await profile.Set(x => x.Gender = newGender).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Fact]
        public async Task GetDoesNotReturnProfileAfterLanguageRemovedTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();
            var languageToRemoved = profile.Languages.First();
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = languageToRemoved
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            profile = await profile.Set(x => x.Languages.Remove(languageToRemoved)).Save(_logger, account)
                .ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Fact]
        public async Task GetDoesNotReturnProfileAfterProfileBannedWhenPreviouslyMatchedFilterTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = profile.Languages.First()
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            await profile.Set(x => x.BannedAt = DateTimeOffset.UtcNow).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
        }

        [Fact]
        public async Task GetDoesNotReturnProfileAfterProfileHiddenTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = profile.Languages.First()
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());

            await profile.Set(x => x.Status = ProfileStatus.Hidden).Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Should().NotContain(x => x.Id == profile.Id);
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
        public async Task GetDoesReturnProfileAfterGenderUpdatedMatchesExistingFilterTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>().ClearSkills().ClearLanguages();
            var newGender = Guid.NewGuid().ToString();
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = newGender
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Should().NotContain(x => x.Id == profile.Id);

            profile.Gender = newGender;

            // Save the gender
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            await profile.Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetIgnoresFiltersOnUnapprovedCategoriesTest()
        {
            var firstProfile = Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Languages.Clear())
                .Set(x => x.Languages.Add("English"));

            await firstProfile.SaveAllCategories(_logger).ConfigureAwait(false);

            firstProfile = await firstProfile.Save(_logger).ConfigureAwait(false);

            var secondProfile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Languages.Clear()).Set(x => x.Languages.Add("English"))
                .Set(x => x.Gender = Guid.NewGuid().ToString()).Save(_logger).ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = firstProfile.Languages.First()
                },
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = secondProfile.Gender
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == firstProfile.Id)
                .ShouldBeEquivalentTo(firstProfile, opt => opt.ExcludingMissingMembers());
            actual.Single(x => x.Id == secondProfile.Id)
                .ShouldBeEquivalentTo(secondProfile, opt => opt.ExcludingMissingMembers().Excluding(x => x.Gender));
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
        public async Task GetReturnProfileAfterLanguageAddedToMatchExistingFilterTest()
        {
            var account = Model.Create<Account>();
            var newLanguage = Guid.NewGuid().ToString();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>().ClearSkills().ClearLanguages();
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = newLanguage
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            // Save the current categories
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Should().NotContain(x => x.Id == profile.Id);

            await profile.Set(x => x.Languages.Add(newLanguage)).Save(_logger, account).ConfigureAwait(false);

            // Save the updated categories
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnProfileAfterSkillAddedToMatchExistingFilterTest()
        {
            var account = Model.Create<Account>();
            var newSkill = Model.Using<ProfileBuildStrategy>().Create<Skill>();
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger, account)
                .ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Skill,
                    CategoryName = newSkill.Name
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            // Save the current categories
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Should().NotContain(x => x.Id == profile.Id);

            await profile.Set(x => x.Skills.Add(newSkill)).Save(_logger, account).ConfigureAwait(false);

            // Save the updated categories
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Theory]
        [InlineData(CategoryGroup.Gender)]
        [InlineData(CategoryGroup.Language)]
        [InlineData(CategoryGroup.Skill)]
        public async Task GetReturnsEmptyWhenNoProfilesMatchFilterTest(CategoryGroup group)
        {
            // Ensure there is at least one profile available
            await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = group,
                    CategoryName = Guid.NewGuid().ToString()
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReturnsGenderCorrectlyWhenToggledBetweenVisibleAndInvisibleTest()
        {
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Gender = Guid.NewGuid().ToString());
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = profile.Languages.Skip(2).First()
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            await profile.SaveAllCategories().ConfigureAwait(false);

            profile = await profile.Save().ConfigureAwait(false);

            var firstActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id).Gender.Should().Be(profile.Gender);

            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var categoryAddress = ApiLocation.Category(CategoryGroup.Gender, profile.Gender);
            var updateCategory = new UpdateCategory
                {Visible = false};

            // Hide the gender category
            await Client.Put(categoryAddress, _logger, updateCategory, administrator, HttpStatusCode.NoContent)
                .ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id).Gender.Should().BeNullOrEmpty();

            updateCategory.Visible = true;

            // Show the gender category again
            await Client.Put(categoryAddress, _logger, updateCategory, administrator, HttpStatusCode.NoContent)
                .ConfigureAwait(false);

            var thirdActual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            thirdActual.Single(x => x.Id == profile.Id).Gender.Should().Be(profile.Gender);
        }

        [Fact]
        public async Task GetReturnsMostRecentDataWhenProfileUpdatedTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>().ClearSkills().ClearLanguages();

            // Save the gender
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

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

            // Save the gender
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            await profile.Save(_logger, account).ConfigureAwait(false);

            var secondActual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger)
                .ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id)
                .ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsMultipleProfilesMatchingFiltersTest()
        {
            var firstProfile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger)
                .ConfigureAwait(false);
            var secondProfile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Gender = firstProfile.Gender).Save(_logger).ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = firstProfile.Gender
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == firstProfile.Id)
                .ShouldBeEquivalentTo(firstProfile, opt => opt.ExcludingMissingMembers());
            actual.Single(x => x.Id == secondProfile.Id)
                .ShouldBeEquivalentTo(secondProfile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsNewGenderAfterApprovedForAnonymousUserTest()
        {
            var account = Model.Using<ProfileBuildStrategy>().Create<Account>();
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>();

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            profile.Gender = Guid.NewGuid().ToString("N");

            profile = await profile.Save(_logger, account).ConfigureAwait(false);

            var firstFilters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Skill,
                    CategoryName = profile.Skills.First().Name
                }
            };
            var firstAddress = ApiLocation.ProfilesMatching(firstFilters);

            var firstActual = await Client.Get<List<ProfileResult>>(firstAddress, _logger).ConfigureAwait(false);

            firstActual.Single(x => x.Id == profile.Id).Gender.Should().BeNull();

            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();

            await new NewCategory {Group = CategoryGroup.Gender, Name = profile.Gender}.Save(_logger, administrator)
                .ConfigureAwait(false);

            var secondFilters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = profile.Gender
                }
            };
            var secondAddress = ApiLocation.ProfilesMatching(secondFilters);

            var secondActual = await Client.Get<List<ProfileResult>>(firstAddress, _logger).ConfigureAwait(false);

            secondActual.Single(x => x.Id == profile.Id).Gender.Should().Be(profile.Gender);
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

        [Fact]
        public async Task GetReturnsProfileWithAllCategoryFiltersAppliedTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger)
                .ConfigureAwait(false);
            var filters = new List<ProfileFilter>();

            if (string.IsNullOrWhiteSpace(profile.Gender))
            {
                filters.Add(
                    new ProfileFilter
                    {
                        CategoryGroup = CategoryGroup.Gender,
                        CategoryName = profile.Gender
                    });
            }

            foreach (var language in profile.Languages)
            {
                filters.Add(
                    new ProfileFilter
                    {
                        CategoryGroup = CategoryGroup.Language,
                        CategoryName = language
                    });
            }

            foreach (var skill in profile.Skills)
            {
                filters.Add(
                    new ProfileFilter
                    {
                        CategoryGroup = CategoryGroup.Skill,
                        CategoryName = skill.Name
                    });
            }

            var address = ApiLocation.ProfilesMatching(filters);

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Theory]
        [InlineData("Female")]
        [InlineData("female")]
        [InlineData("FEMALE")]
        public async Task GetReturnsProfileWithCaseInsensitiveFilterMatchTest(string filterName)
        {
            var category = new NewCategory
            {
                Group = CategoryGroup.Gender,
                Name = "Female"
            };

            await category.Save(_logger).ConfigureAwait(false);

            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Set(x => x.Gender = "Female")
                .Save(_logger).ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = filterName
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsProfileWithGenderFilterTest()
        {
            var profile = Model.Using<ProfileBuildStrategy>().Create<Profile>().ClearSkills().ClearLanguages();

            // Save the gender
            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            profile = await profile.Save(_logger).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Gender,
                    CategoryName = profile.Gender
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsProfileWithLanguageFilterTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger)
                .ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Language,
                    CategoryName = profile.Languages.Skip(2).First()
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsProfileWithoutAnyCategoryLinksWhenNoFiltersAppliedTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().ClearCategories().Save(_logger)
                .ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(ApiLocation.Profiles, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsProfileWithoutGenderWhenGenderIsUnapprovedTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>()
                .Set(x => x.Gender = null)
                .ClearLanguages().Save(_logger)
                .ConfigureAwait(false);

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            // Save a gender that is not approved
            profile = await profile.Set(x => x.Gender = Guid.NewGuid().ToString()).Save(_logger).ConfigureAwait(false);

            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Skill,
                    CategoryName = profile.Skills.Skip(2).First().Name
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            var result = actual.Single(x => x.Id == profile.Id);

            result.ShouldBeEquivalentTo(profile, opt => opt.Excluding(x => x.Gender).ExcludingMissingMembers());
            result.Gender.Should().BeNull();
        }

        [Fact]
        public async Task GetReturnsProfileWithSkillFilterTest()
        {
            var profile = await Model.Using<ProfileBuildStrategy>().Create<Profile>().Save(_logger)
                .ConfigureAwait(false);
            var filters = new List<ProfileFilter>
            {
                new ProfileFilter
                {
                    CategoryGroup = CategoryGroup.Skill,
                    CategoryName = profile.Skills.Skip(2).First().Name
                }
            };
            var address = ApiLocation.ProfilesMatching(filters);

            await profile.SaveAllCategories(_logger).ConfigureAwait(false);

            var actual = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);

            actual.Single(x => x.Id == profile.Id).ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public async Task GetReturnsResultsWithExpectedSortOrderTest()
        {
            var source = await Model.Using<ProfileBuildStrategy>().Create<List<Profile>>().SetEach(
                x =>
                {
                    x.Gender = null;
                    x.Languages.Clear();
                    x.Skills.Clear();
                }).Save().ConfigureAwait(false);

            var expected = (from x in source
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ??
                                                                                  DateTimeOffset.UtcNow.Year
                select x.Id).ToList();

            var address = ApiLocation.Profiles;

            var results = await Client.Get<List<ProfileResult>>(address, _logger).ConfigureAwait(false);
            var actual = new List<ProfileResult>(expected.Count);

            foreach (var result in results)
            {
                if (expected.Any(x => x == result.Id))
                {
                    actual.Add(result);
                }
            }

            actual.Select(x => x.Id).Should().ContainInOrder(expected);
        }
    }
}