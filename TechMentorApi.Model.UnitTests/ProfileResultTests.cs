namespace TechMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class ProfileResultTests
    {
        [Fact]
        public void CanCreateDefaultInstanceTest()
        {
            var sut = new ProfileResult();

            sut.Status.Should().Be(default(ProfileStatus));
            sut.Id.Should().BeEmpty();
            sut.BirthYear.Should().NotHaveValue();
            sut.FirstName.Should().BeNull();
            sut.Gender.Should().BeNull();
            sut.LastName.Should().BeNull();
            sut.TimeZone.Should().BeNull();
            sut.YearStartedInTech.Should().NotHaveValue();
        }

        [Fact]
        public void CanCreateFromProfileTest()
        {
            var profile = Model.Create<Profile>();

            var sut = new ProfileResult(profile);

            sut.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }
    }
}