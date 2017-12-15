namespace TechMentorApi.Model.UnitTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class PublicProfileTests
    {
        [Fact]
        public void CanCreateDefaultProfileTest()
        {
            var sut = new PublicProfile();

            sut.About.Should().BeNull();
            sut.PhotoHash.Should().BeNull();
            sut.PhotoHash.Should().BeNull();
            sut.BirthYear.Should().BeNull();
            sut.FirstName.Should().BeNull();
            sut.Gender.Should().BeNull();
            sut.GitHubUsername.Should().BeNull();
            sut.Id.Should().BeEmpty();
            sut.Languages.Should().BeEmpty();
            sut.LastName.Should().BeNull();
            sut.Skills.Should().BeEmpty();
            sut.Status.Should().Be(ProfileStatus.Hidden);
            sut.TimeZone.Should().BeNull();
            sut.TwitterUsername.Should().BeNull();
            sut.Website.Should().BeNull();
            sut.YearStartedInTech.Should().BeNull();
        }

        [Fact]
        public void CopiesDataFromProfileTest()
        {
            var profile = Model.Create<Profile>();

            var sut = new PublicProfile(profile);

            sut.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void CopiesSkillValuesInsteadOfCopyingReferenceTest()
        {
            var profile = Model.Create<Profile>();

            var sut = new PublicProfile(profile);

            sut.Skills.Should().NotBeSameAs(profile.Skills);

            foreach (var skill in profile.Skills)
            {
                var matchingSkill = sut.Skills.Single(x => x.Name == skill.Name);

                skill.Should().NotBeSameAs(matchingSkill);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileTest()
        {
            Action action = () => new PublicProfile(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}