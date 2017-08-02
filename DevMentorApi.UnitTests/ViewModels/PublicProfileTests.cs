namespace DevMentorApi.UnitTests.ViewModels
{
    using System;
    using DevMentorApi.Model;
    using DevMentorApi.ViewModels;
    using FluentAssertions;
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
            sut.Status.Should().Be(ProfileStatus.Unavailable);
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