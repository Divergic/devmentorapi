namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class ProfileTests
    {
        [Fact]
        public void CopiesAllInformationWhenCreatedWithUpdatableProfileTest()
        {
            var source = Model.Create<UpdatableProfile>();

            var sut = new Profile(source);

            sut.ShouldBeEquivalentTo(source, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void IsCreatedWithDefaultValuesTest()
        {
            var sut = new Profile();

            sut.Languages.Should().NotBeNull();
            sut.Skills.Should().NotBeNull();
            sut.Status.Should().Be(ProfileStatus.Hidden);
        }

        [Fact]
        public void LanguagesCreatesNewInstanceWhenAssignedNullTest()
        {
            var sut = new Profile();

            sut.Languages = null;

            sut.Languages.Should().NotBeNull();
        }

        [Fact]
        public void SkillsCreatesNewInstanceWhenAssignedNullTest()
        {
            var sut = new Profile();

            sut.Skills = null;

            sut.Skills.Should().NotBeNull();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullUpdatableProfileTest()
        {
            Action action = () => new Profile(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}