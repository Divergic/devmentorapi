namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class SkillTests
    {
        [Fact]
        public void CanCreateDefaultSkillTest()
        {
            var sut = new Skill();

            sut.Name.Should().BeNull();
            sut.Level.Should().Be(default(SkillLevel));
            sut.YearLastUsed.Should().BeNull();
            sut.YearStarted.Should().BeNull();
        }

        [Fact]
        public void CopiesDataFromSkillTest()
        {
            var skill = Model.Create<Skill>();

            var sut = new Skill(skill);

            sut.Should().BeEquivalentTo(skill, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullSkillTest()
        {
            Action action = () => new Skill(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}