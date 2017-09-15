namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class NotFoundExceptionTests
    {
        [Fact]
        public void CanBeCreatedWithDefaultValuesTest()
        {
            var sut = new NotFoundException();

            sut.Message.Should().NotBeNullOrEmpty();
            sut.InnerException.Should().BeNull();
        }

        [Fact]
        public void CanBeCreatedWithMessageAndInnerExceptionTest()
        {
            var message = Guid.NewGuid().ToString();
            var inner = new TimeoutException();

            var sut = new NotFoundException(message, inner);

            sut.Message.Should().Be(message);
            sut.InnerException.Should().Be(inner);
        }

        [Fact]
        public void CanBeCreatedWithMessageTest()
        {
            var message = Guid.NewGuid().ToString();

            var sut = new NotFoundException(message);

            sut.Message.Should().Be(message);
            sut.InnerException.Should().BeNull();
        }
    }
}