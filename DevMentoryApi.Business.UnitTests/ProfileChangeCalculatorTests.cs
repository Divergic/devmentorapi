namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class ProfileChangeCalculatorTests
    {
        private readonly ITestOutputHelper _output;

        public ProfileChangeCalculatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CalculateChangesAddsNewLanguageTest()
        {
            var original = Model.Create<Profile>();
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
            var original = Model.Create<Profile>();
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
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Unavailable, false)]
        [InlineData(ProfileStatus.Unavailable, ProfileStatus.Available, true)]
        public void CalculateChangesCorrectlyIdentifiesChangesToStatusTest(
            ProfileStatus originalValue,
            ProfileStatus updatedValue,
            bool expected)
        {
            var original = Model.Create<Profile>().Set(x => x.Status = originalValue);
            var updated = original.Clone().Set(x => x.Status = updatedValue);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.CategoryChanges.Should().BeEmpty();
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

            var original = Model.Create<Profile>().Set(x => x.Gender = originalValue);
            var updated = original.Clone().Set(x => x.Gender = updatedValue);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(isChanged);

            if (isAdded == false &&
                isRemoved == false)
            {
                actual.CategoryChanges.Should().HaveCount(0);
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

        [Fact]
        public void CalculateChangesIgnoresLanguageDifferentByCaseOnlyTest()
        {
            var original = Model.Create<Profile>();
            var updated = original.Clone();
            var language = Guid.NewGuid().ToString();

            original.Languages.Add(language.ToLowerInvariant());
            updated.Languages.Add(language.ToUpperInvariant());

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().HaveCount(0);
        }

        [Fact]
        public void CalculateChangesIgnoresSkillNameDifferentByCaseOnlyTest()
        {
            var original = Model.Create<Profile>();
            var updated = original.Clone();
            var oldSkill = Model.Create<Skill>().Set(x => x.Name = x.Name.ToLowerInvariant());
            var newSkill = oldSkill.Clone().Set(x => x.Name = oldSkill.Name.ToUpperInvariant());

            original.Skills.Add(oldSkill);
            updated.Skills.Add(newSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().HaveCount(0);
        }

        [Fact]
        public void CalculateChangesRemovesOldLanguageTest()
        {
            var original = Model.Create<Profile>();
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
            var original = Model.Create<Profile>();
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
        public void CalculateChangesReturnsNoChangesWhenProfilesMatchTest()
        {
            var original = Model.Create<Profile>();
            var updated = original.Clone();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Theory]
        [InlineData(SkillLevel.Beginner, SkillLevel.Beginner, false)]
        [InlineData(SkillLevel.Beginner, SkillLevel.Expert, true)]
        public void CalculateChangesSetsProfileAsChangedWithoutCategoryChangeWhenSkillLevelChangesTest(
            SkillLevel originalLevel,
            SkillLevel updatedLevel,
            bool expected)
        {
            var original = Model.Create<Profile>();
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
            actual.CategoryChanges.Should().HaveCount(0);
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
            var original = Model.Create<Profile>();
            var updated = original.Clone();
            var originalSkill = Model.Create<Skill>().Set(x => x.YearLastUsed = originalValue);
            var updatedSkill = originalSkill.Clone().Set(x => x.YearLastUsed = updatedValue);

            original.Skills.Add(originalSkill);
            updated.Skills.Add(updatedSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
            actual.CategoryChanges.Should().HaveCount(0);
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
            var original = Model.Create<Profile>();
            var updated = original.Clone();
            var originalSkill = Model.Create<Skill>().Set(x => x.YearStarted = originalValue);
            var updatedSkill = originalSkill.Clone().Set(x => x.YearStarted = updatedValue);

            original.Skills.Add(originalSkill);
            updated.Skills.Add(updatedSkill);

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().Be(expected);
            actual.CategoryChanges.Should().HaveCount(0);
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWhenProfileIsNotMatchingTest()
        {
            var original = Model.Create<Profile>();
            var updated = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(original, updated);

            action.ShouldThrow<ArgumentException>();
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
            var original = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(original, null);

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
            var original = Model.Create<Profile>();
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
            PropertyInfo property,
            string originalValue,
            string updatedValue,
            bool expected,
            string description)
        {
            // Return all the test evaluation scenarios for this property
            var original = Model.Create<Profile>();
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

        private static IEnumerable<object[]> IntPropertiesDataSource()
        {
            var properties = from x in typeof(Profile).GetProperties()
                where x.PropertyType == typeof(int?)
                select x;

            foreach (var property in properties)
            {
                var value = Environment.TickCount;

                yield return BuildIntPropertyTestScenario(property, null, null, false, "values are both null");
                yield return BuildIntPropertyTestScenario(property, value, value, false, "values are same");
                yield return BuildIntPropertyTestScenario(
                    property,
                    Environment.TickCount,
                    Environment.TickCount + 1,
                    true,
                    "values are different");
                yield return BuildIntPropertyTestScenario(property, null, value, true, "values changing from null");
                yield return BuildIntPropertyTestScenario(property, value, null, true, "values changing to null");
            }
        }

        private static IEnumerable<object[]> StringPropertiesDataSource()
        {
            var properties = from x in typeof(Profile).GetProperties()
                where x.PropertyType == typeof(string) && x.Name != nameof(Profile.Gender)
                select x;

            foreach (var property in properties)
            {
                var value = Guid.NewGuid().ToString();

                yield return BuildStringPropertyTestScenario(property, value, value, false, "values are same");

                yield return BuildStringPropertyTestScenario(property, null, null, false, "values are both null");

                yield return BuildStringPropertyTestScenario(
                    property,
                    string.Empty,
                    string.Empty,
                    false,
                    "values are both empty");

                yield return BuildStringPropertyTestScenario(property, " ", " ", false, "values are both whitespace");

                yield return BuildStringPropertyTestScenario(
                    property,
                    null,
                    string.Empty,
                    false,
                    "values are null and empty");

                yield return BuildStringPropertyTestScenario(
                    property,
                    null,
                    " ",
                    false,
                    "values are null and whitespace");

                yield return BuildStringPropertyTestScenario(
                    property,
                    string.Empty,
                    null,
                    false,
                    "values are empty and null");

                yield return BuildStringPropertyTestScenario(
                    property,
                    " ",
                    null,
                    false,
                    "values are whitespace and null");

                yield return BuildStringPropertyTestScenario(
                    property,
                    " ",
                    string.Empty,
                    false,
                    "values are whitespace and empty");

                yield return BuildStringPropertyTestScenario(
                    property,
                    string.Empty,
                    " ",
                    false,
                    "values are empty and whitespace");

                yield return BuildStringPropertyTestScenario(
                    property,
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    true,
                    "values are different");

                yield return BuildStringPropertyTestScenario(
                    property,
                    value.ToLowerInvariant(),
                    value.ToUpperInvariant(),
                    true,
                    "values are same but with different case");

                yield return BuildStringPropertyTestScenario(
                    property,
                    null,
                    value.ToUpperInvariant(),
                    true,
                    "value changed from null");

                yield return BuildStringPropertyTestScenario(
                    property,
                    value.ToUpperInvariant(),
                    null,
                    true,
                    "value changed to null");

                yield return BuildStringPropertyTestScenario(
                    property,
                    string.Empty,
                    value.ToUpperInvariant(),
                    true,
                    "value changed from empty");

                yield return BuildStringPropertyTestScenario(
                    property,
                    value.ToUpperInvariant(),
                    string.Empty,
                    true,
                    "value changed to empty");

                yield return BuildStringPropertyTestScenario(
                    property,
                    " ",
                    value.ToUpperInvariant(),
                    true,
                    "value changed from whitespace");

                yield return BuildStringPropertyTestScenario(
                    property,
                    value.ToUpperInvariant(),
                    " ",
                    true,
                    "value changed to whitespace");
            }
        }
    }
}