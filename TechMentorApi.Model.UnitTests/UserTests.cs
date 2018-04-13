namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class UserTests
    {
        [Fact]
        public void CanCreateUserWithUsernameTest()
        {
            var username = Guid.NewGuid().ToString();

            var sut = new User(username);

            sut.Username.Should().Be(username);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsExceptionWhenCreatedWithInvalidUsernameTest(string username)
        {
            Action action = () => new User(username);

            action.Should().Throw<ArgumentException>();
        }
    }
}