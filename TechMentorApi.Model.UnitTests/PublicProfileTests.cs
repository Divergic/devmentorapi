namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using Model;
    using ModelBuilder;
    using Xunit;

    public class PublicProfileTests
    {
        [Fact]
        public void CanCreateDefaultProfileTest()
        {
            var sut = new PublicProfile();

            sut.Id.Should().BeEmpty();
            sut.FirstName.Should().BeNull();
            sut.LastName.Should().BeNull();
            sut.Status.Should().Be(ProfileStatus.Hidden);
        }

        [Fact]
        public void CopiesDataFromProfileTest()
        {
            var profile = Model.Create<Profile>();

            var sut = new PublicProfile(profile);

            sut.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileTest()
        {
            Action action = () => new PublicProfile(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}