namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using ModelBuilder;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;
    using Xunit.Abstractions;

    public class ProfileChangeCalculatorTests
    {
        private readonly ITestOutputHelper _output;

        public ProfileChangeCalculatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> IntPropertiesDataSource()
        {
            var scenarios = new List<object[]>();
            var properties = from x in typeof(Profile).GetProperties()
                where x.PropertyType == typeof(int?)
                select x;

            foreach (var property in properties)
            {
                var value = Environment.TickCount;

                scenarios.Add(BuildIntPropertyTestScenario(property, null, null, false, "values are both null"));
                scenarios.Add(BuildIntPropertyTestScenario(property, value, value, false, "values are same"));
                scenarios.Add(
                    BuildIntPropertyTestScenario(
                        property,
                        Environment.TickCount,
                        Environment.TickCount + 1,
                        true,
                        "values are different"));
                scenarios.Add(BuildIntPropertyTestScenario(property, null, value, true, "values changing from null"));
                scenarios.Add(BuildIntPropertyTestScenario(property, value, null, true, "values changing to null"));
            }

            return scenarios;
        }

        public static IEnumerable<object[]> StringPropertiesDataSource()
        {
            var template = Model.Create<Profile>();
            var scenarios = new List<object[]>();
            var properties = (from x in typeof(Profile).GetProperties()
                where x.PropertyType == typeof(string) && x.Name != nameof(Profile.Gender)
                select x).ToList();

            foreach (var property in properties)
            {
                var value = Guid.NewGuid().ToString();

                scenarios.Add(
                    BuildStringPropertyTestScenario(template, property, value, value, false, "values are same"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(template, property, null, null, false, "values are both null"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        string.Empty,
                        string.Empty,
                        false,
                        "values are both empty"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(template, property, " ", " ", false, "values are both whitespace"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        null,
                        string.Empty,
                        false,
                        "values are null and empty"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        null,
                        " ",
                        false,
                        "values are null and whitespace"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        string.Empty,
                        null,
                        false,
                        "values are empty and null"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        " ",
                        null,
                        false,
                        "values are whitespace and null"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        " ",
                        string.Empty,
                        false,
                        "values are whitespace and empty"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        string.Empty,
                        " ",
                        false,
                        "values are empty and whitespace"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(),
                        true,
                        "values are different"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        value.ToLowerInvariant(),
                        value.ToUpperInvariant(),
                        true,
                        "values are same but with different case"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        null,
                        value.ToUpperInvariant(),
                        true,
                        "value changed from null"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        value.ToUpperInvariant(),
                        null,
                        true,
                        "value changed to null"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        string.Empty,
                        value.ToUpperInvariant(),
                        true,
                        "value changed from empty"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        value.ToUpperInvariant(),
                        string.Empty,
                        true,
                        "value changed to empty"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        " ",
                        value.ToUpperInvariant(),
                        true,
                        "value changed from whitespace"));

                scenarios.Add(
                    BuildStringPropertyTestScenario(
                        template,
                        property,
                        value.ToUpperInvariant(),
                        " ",
                        true,
                        "value changed to whitespace"));
            }

            return scenarios;
        }

        [Fact]
        public void CalculateChangesAddingGenderOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.Gender = null);
            var updated = original.Clone().Set(x => x.Gender = "Female");

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesAddingGenderOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.Gender = null);
            var updated = original.Clone().Set(x => x.Gender = "Female");

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesAddingLanguageOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            updated.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesAddingLanguageOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            updated.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesAddingSkillOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            updated.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesAddingSkillOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            updated.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public void CalculateChangesAddsAllUpdatedCategoriesWhenStatusChangedFromHiddenTest(ProfileStatus status)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone().Set(x => x.Status = status);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            // + 1 for gender
            var expectedChangeCount = updated.Skills.Count + updated.Languages.Count + 1;
            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(expectedChangeCount);

            var skillsRemoved = updated.Skills.Select(x => x.Name);

            actual.CategoryChanges.All(x => x.ChangeType == CategoryLinkChangeType.Add).Should().BeTrue();
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Skill).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(skillsRemoved);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Language).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(updated.Languages);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Gender).Select(x => x.CategoryName)
                .Single()
                .Should().Be(updated.Gender);
        }

        [Fact]
        public void CalculateChangesAddsNewLanguageTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            updated.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(1);

            var change = actual.CategoryChanges.First();

            change.CategoryGroup.Should().Be(CategoryGroup.Language);
            change.CategoryName.Should().Be(language);
            change.ChangeType.Should().Be(CategoryLinkChangeType.Add);
        }

        [Fact]
        public void CalculateChangesAddsNewSkillTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            updated.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(1);

            var change = actual.CategoryChanges.First();

            change.CategoryGroup.Should().Be(CategoryGroup.Skill);
            change.CategoryName.Should().Be(skill.Name);
            change.ChangeType.Should().Be(CategoryLinkChangeType.Add);
        }

        [Theory]
        [MemberData(nameof(IntPropertiesDataSource))]
        public void CalculateChangesCorrectlyIdentifiesChangesToIntPropertiesTest(
            Profile original,
            Profile updated,
            bool expected,
            string scenario)
        {
            _output.WriteLine(scenario);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.CategoryChanges.Should().BeEmpty();
            actual.ProfileChanged.Should().Be(expected);
        }

        [Theory]
        [InlineData(ProfileStatus.Hidden, ProfileStatus.Hidden, false)]
        [InlineData(ProfileStatus.Available, ProfileStatus.Available, false)]
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Unavailable, false)]
        [InlineData(ProfileStatus.Hidden, ProfileStatus.Available, true)]
        [InlineData(ProfileStatus.Hidden, ProfileStatus.Unavailable, true)]
        [InlineData(ProfileStatus.Available, ProfileStatus.Hidden, true)]
        [InlineData(ProfileStatus.Available, ProfileStatus.Unavailable, true)]
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Available, true)]
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Hidden, true)]
        public void CalculateChangesCorrectlyIdentifiesChangesToStatusTest(
            ProfileStatus originalValue,
            ProfileStatus updatedValue,
            bool expected)
        {
            var original = Model.Create<Profile>().Set(x => x.Status = originalValue);
            var updated = original.Clone().Set(x => x.Status = updatedValue);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(StringPropertiesDataSource))]
        public void CalculateChangesCorrectlyIdentifiesChangesToStringPropertiesTest(
            Profile original,
            Profile updated,
            bool expected,
            string scenario)
        {
            _output.WriteLine(scenario);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.CategoryChanges.Should().BeEmpty();
            actual.ProfileChanged.Should().Be(expected);
        }

        [Theory]
        [InlineData("Female", "Female", false, false, false, "Value is the same")]
        [InlineData(null, null, false, false, false, "No change from null to null")]
        [InlineData(null, "", false, false, false, "No change from null to empty")]
        [InlineData(null, " ", false, false, false, "No change from null to whitespace")]
        [InlineData("", null, false, false, false, "No change from empty to null")]
        [InlineData(" ", null, false, false, false, "No change from whitespace to null")]
        [InlineData("", "", false, false, false, "No change from empty to empty")]
        [InlineData("  ", "  ", false, false, false, "No change from whitespace to whitespace")]
        [InlineData("  ", "", false, false, false, "No change from whitespace to empty")]
        [InlineData("", "  ", false, false, false, "No change from empty to whitespace")]
        [InlineData("Female", "FEMALE", false, false, false, "Value is the same but different case")]
        [InlineData("Female", "Male", true, true, true, "Value is changed")]
        [InlineData(null, "Female", true, true, false, "Value is added to null")]
        [InlineData("", "Female", true, true, false, "Value is added to empty")]
        [InlineData(" ", "Female", true, true, false, "Value is added to whitespace")]
        [InlineData("Female", null, true, false, true, "Value is remove to null")]
        [InlineData("Female", "", true, false, true, "Value is removed to empty")]
        [InlineData("Female", " ", true, false, true, "Value is removed to whitespace")]
        public void CalculateChangesDetectsChangesToGenderTest(
            string originalValue,
            string updatedValue,
            bool isChanged,
            bool isAdded,
            bool isRemoved,
            string scenario)
        {
            _output.WriteLine("{0}: [{1}], [{2}]", scenario, originalValue ?? "null", updatedValue ?? "null");

            var original = Model.Create<Profile>().Set(x => x.Gender = originalValue).Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone().Set(x => x.Gender = updatedValue);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(isChanged);

            if (isAdded == false &&
                isRemoved == false)
            {
                actual.CategoryChanges.Should().BeEmpty();
            }
            else if (isAdded && isRemoved)
            {
                actual.CategoryChanges.Should().HaveCount(2);
                actual.CategoryChanges.All(x => x.CategoryGroup == CategoryGroup.Gender).Should().BeTrue();
                actual.CategoryChanges.Should().Contain(
                    x => x.ChangeType == CategoryLinkChangeType.Remove && x.CategoryName == originalValue);
                actual.CategoryChanges.Should().Contain(
                    x => x.ChangeType == CategoryLinkChangeType.Add && x.CategoryName == updatedValue);
            }
            else if (isAdded)
            {
                actual.CategoryChanges.Should().HaveCount(1);
                actual.CategoryChanges.All(x => x.CategoryGroup == CategoryGroup.Gender).Should().BeTrue();
                actual.CategoryChanges.Should().Contain(
                    x => x.ChangeType == CategoryLinkChangeType.Add && x.CategoryName == updatedValue);
            }
            else
            {
                actual.CategoryChanges.Should().HaveCount(1);
                actual.CategoryChanges.All(x => x.CategoryGroup == CategoryGroup.Gender).Should().BeTrue();
                actual.CategoryChanges.Should().Contain(
                    x => x.ChangeType == CategoryLinkChangeType.Remove && x.CategoryName == originalValue);
            }
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public void CalculateChangesIdentifiesAllCategoriesRemovedWhenStatusChangedToHiddenTest(ProfileStatus status)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = status);
            var updated = original.Clone().Set(x => x.Status = ProfileStatus.Hidden);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            // + 1 for gender
            var expectedChangeCount = original.Skills.Count + original.Languages.Count + 1;
            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(expectedChangeCount);

            var skillsRemoved = original.Skills.Select(x => x.Name);

            actual.CategoryChanges.All(x => x.ChangeType == CategoryLinkChangeType.Remove).Should().BeTrue();
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Skill).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(skillsRemoved);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Language).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(original.Languages);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Gender).Select(x => x.CategoryName)
                .Single()
                .Should().Be(original.Gender);
        }

        [Fact]
        public void CalculateChangesIgnoresGenderDifferentByCaseOnlyTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var gender = Guid.NewGuid().ToString();

            original.Gender = gender.ToLowerInvariant();
            updated.Gender = gender.ToUpperInvariant();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesIgnoresLanguageDifferentByCaseOnlyTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            original.Languages.Add(language.ToLowerInvariant());
            updated.Languages.Add(language.ToUpperInvariant());

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public void CalculateChangesIgnoresNewCategoriesWhenStatusChangedToHiddenTest(ProfileStatus status)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = status)
                .Set(x => x.Skills.Clear())
                .Set(x => x.Languages.Clear())
                .Set(x => x.Gender = null);
            var updated = original.Clone().Set(x => x.Status = ProfileStatus.Hidden);
            var skill = Model.Create<Skill>();

            updated.Skills.Add(skill);
            updated.Languages.Add(Guid.NewGuid().ToString());
            updated.Gender = Guid.NewGuid().ToString();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public void CalculateChangesIgnoresOriginalCategoriesWhenStatusChangedFromHiddenTest(ProfileStatus status)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone().Set(x => x.Status = status);
            var skill = Model.Create<Skill>();

            original.Skills.Add(skill);
            original.Languages.Add(Guid.NewGuid().ToString());
            original.Gender = Guid.NewGuid().ToString();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            // + 1 for gender
            var expectedChangeCount = updated.Skills.Count + updated.Languages.Count + 1;
            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(expectedChangeCount);

            var skillsRemoved = updated.Skills.Select(x => x.Name);

            actual.CategoryChanges.All(x => x.ChangeType == CategoryLinkChangeType.Add).Should().BeTrue();
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Skill).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(skillsRemoved);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Language).Select(x => x.CategoryName)
                .Should().BeEquivalentTo(updated.Languages);
            actual.CategoryChanges.Where(x => x.CategoryGroup == CategoryGroup.Gender).Select(x => x.CategoryName)
                .Single()
                .Should().Be(updated.Gender);
        }

        [Fact]
        public void CalculateChangesIgnoresSkillNameDifferentByCaseOnlyTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var oldSkill = Model.Create<Skill>().Set(x => x.Name = x.Name.ToLowerInvariant());
            var newSkill = oldSkill.Clone().Set(x => x.Name = oldSkill.Name.ToUpperInvariant());

            original.Skills.Add(oldSkill);
            updated.Skills.Add(newSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(ProfileStatus.Available, ProfileStatus.Unavailable)]
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Available)]
        public void
            CalculateChangesMarksProfileChangedWithNoCategoryChangesWhenStatusChangedBetweenAvailableAndUnavailableTest(
                ProfileStatus originalValue,
                ProfileStatus updatedValue)
        {
            var original = Model.Create<Profile>().Set(x => x.Status = originalValue);
            var updated = original.Clone().Set(x => x.Status = updatedValue);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovesOldLanguageTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            original.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(1);

            var change = actual.CategoryChanges.First();

            change.CategoryGroup.Should().Be(CategoryGroup.Language);
            change.CategoryName.Should().Be(language);
            change.ChangeType.Should().Be(CategoryLinkChangeType.Remove);
        }

        [Fact]
        public void CalculateChangesRemovesOldSkillTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            original.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().HaveCount(1);

            var change = actual.CategoryChanges.First();

            change.CategoryGroup.Should().Be(CategoryGroup.Skill);
            change.CategoryName.Should().Be(skill.Name);
            change.ChangeType.Should().Be(CategoryLinkChangeType.Remove);
        }

        [Fact]
        public void CalculateChangesRemovingOldGenderOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone().Set(x => x.Gender = null);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovingOldGenderOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone().Set(x => x.Gender = null);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovingOldLanguageOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            original.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovingOldLanguageOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            original.Languages.Add(language);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovingOldSkillOnAccountStillHiddenMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Hidden);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            original.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesRemovingOldSkillOnBannedAccountMarksProfileAsChangedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            original.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesReturnsNoChangesWhenProfilesMatchTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesSetsProfileAsChangedButWithoutCategoryChangesWhenProfileIsBannedTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);
            var updated = original.Clone();
            var skill = Model.Create<Skill>();

            updated.Skills.Add(skill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeTrue();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SkillLevel.Beginner, SkillLevel.Beginner, false)]
        [InlineData(SkillLevel.Beginner, SkillLevel.Expert, true)]
        public void CalculateChangesSetsProfileAsChangedWithCategoryChangeWhenSkillLevelChangesTest(
            SkillLevel originalLevel,
            SkillLevel updatedLevel,
            bool expected)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var originalSkill = Model.Create<Skill>().Set(x => x.Level = originalLevel);
            var updatedSkill = originalSkill.Clone().Set(x => x.Level = updatedLevel);

            original.Skills.Clear();
            updated.Skills.Clear();
            original.Skills.Add(originalSkill);
            updated.Skills.Add(updatedSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(1, 1, false)]
        [InlineData(1, 2, true)]
        [InlineData(1, null, true)]
        [InlineData(null, 1, true)]
        public void CalculateChangesSetsProfileAsChangedWithoutCategoryChangeWhenYearLastUsedChangesTest(
            int? originalValue,
            int? updatedValue,
            bool expected)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var originalSkill = Model.Create<Skill>().Set(x => x.YearLastUsed = originalValue);
            var updatedSkill = originalSkill.Clone().Set(x => x.YearLastUsed = updatedValue);

            original.Skills.Add(originalSkill);
            updated.Skills.Add(updatedSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(1, 1, false)]
        [InlineData(1, 2, true)]
        [InlineData(1, null, true)]
        [InlineData(null, 1, true)]
        public void CalculateChangesSetsProfileAsChangedWithoutCategoryChangeWhenYearStartedChangesTest(
            int? originalValue,
            int? updatedValue,
            bool expected)
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();
            var originalSkill = Model.Create<Skill>().Set(x => x.YearStarted = originalValue);
            var updatedSkill = originalSkill.Clone().Set(x => x.YearStarted = updatedValue);

            original.Skills.Add(originalSkill);
            updated.Skills.Add(updatedSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWithNullOriginalProfileTest()
        {
            var updated = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(null, updated);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWithNullUpdatedProfileTest()
        {
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(original, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData("Female", true)]
        [InlineData(null, false)]
        public void RemoveAllCategoryLinksDeterminesGenderChangeTest(string gender, bool removeExpected)
        {
            var profile = Model.Create<Profile>().Set(x => x.Gender = gender);

            var sut = new ProfileChangeCalculator();

            var actual = sut.RemoveAllCategoryLinks(profile);

            if (removeExpected)
            {
                actual.CategoryChanges.Should().Contain(
                    x => x.CategoryGroup == CategoryGroup.Gender && x.CategoryName == gender);
            }
            else
            {
                actual.CategoryChanges.Should().NotContain(
                    x => x.CategoryGroup == CategoryGroup.Gender && x.CategoryName == gender);
            }
        }

        [Fact]
        public void RemoveAllCategoryLinksDeterminesSkillAndLanguageChangesTest()
        {
            var profile = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            var actual = sut.RemoveAllCategoryLinks(profile);

            foreach (var language in profile.Languages)
            {
                actual.CategoryChanges.Should().Contain(
                    x => x.CategoryGroup == CategoryGroup.Language && x.CategoryName == language);
            }

            foreach (var skill in profile.Skills)
            {
                actual.CategoryChanges.Should().Contain(
                    x => x.CategoryGroup == CategoryGroup.Skill && x.CategoryName == skill.Name);
            }
        }

        [Fact]
        public void RemoveAllCategoryLinksIgnoresChangesWhenNoLanguagesLinkedTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.Languages.Clear());

            var sut = new ProfileChangeCalculator();

            var actual = sut.RemoveAllCategoryLinks(profile);

            actual.CategoryChanges.Should().NotContain(x => x.CategoryGroup == CategoryGroup.Language);
        }

        [Fact]
        public void RemoveAllCategoryLinksIgnoresChangesWhenNoSkillsLinkedTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.Skills.Clear());

            var sut = new ProfileChangeCalculator();

            var actual = sut.RemoveAllCategoryLinks(profile);

            actual.CategoryChanges.Should().NotContain(x => x.CategoryGroup == CategoryGroup.Skill);
        }

        [Fact]
        public void RemoveAllCategoryLinksThrowsExceptionWithNullProfileTest()
        {
            var sut = new ProfileChangeCalculator();

            Action action = () => sut.RemoveAllCategoryLinks(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        private static object[] BuildIntPropertyTestScenario(
            PropertyInfo property,
            int? originalValue,
            int? updatedValue,
            bool expected,
            string description)
        {
            // Return all the test evaluation scenarios for this property
            var original = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var updated = original.Clone();

            property.SetValue(original, originalValue);
            property.SetValue(updated, updatedValue);

            var scenario = property.Name + " ([" + (originalValue.HasValue ? originalValue.ToString() : "null") +
                           "], [" + (updatedValue.HasValue ? updatedValue.ToString() : "null") + "]) " + description;

            return new object[]
            {
                original,
                updated,
                expected,
                scenario
            };
        }

        private static object[] BuildStringPropertyTestScenario(
            Profile template,
            PropertyInfo property,
            string originalValue,
            string updatedValue,
            bool expected,
            string description)
        {
            // Return all the test evaluation scenarios for this property
            var original = template.Clone().Set(x => x.BannedAt = null);
            var updated = original.Clone();

            property.SetValue(original, originalValue);
            property.SetValue(updated, updatedValue);

            var scenario = property.Name + " ([" + (originalValue ?? "null") + "], [" + (updatedValue ?? "null") +
                           "]) " + description;

            return new object[]
            {
                original,
                updated,
                expected,
                scenario
            };
        }
    }
}