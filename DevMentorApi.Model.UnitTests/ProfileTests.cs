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
    }
}