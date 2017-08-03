namespace DevMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using Xunit;

    public class ProfileTests
    {
        [Fact]
        public void IsCreatedWithDefaultValuesTest()
        {
            var sut = new Profile();

            sut.Languages.Should().NotBeNull();
            sut.Skills.Should().NotBeNull();
            sut.Status.Should().Be(ProfileStatus.Unavailable);
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
    }
}