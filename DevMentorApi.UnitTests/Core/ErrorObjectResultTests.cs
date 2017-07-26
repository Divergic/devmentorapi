namespace DevMentorApi.UnitTests.Core
{
    using System;
    using System.Net;
    using System.Reflection;
    using DevMentorApi.Core;
    using FluentAssertions;
    using Xunit;

    public class ErrorObjectResultTests
    {
        [Fact]
        public void BuildsErrorWithInternalServerErrorAndShieldMessageWhenObjectIsNullTest()
        {
            var sut = new ErrorObjectResult((object)null);

            sut.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void BuildsErrorWithInternalServerErrorAndSpecifiedMessageTest()
        {
            var message = Guid.NewGuid().ToString();

            var sut = new ErrorObjectResult(message);

            sut.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().Be(message);
        }

        [Fact]
        public void BuildsErrorWithInternalServerErrorAndSpecifiedObjectTest()
        {
            var error = new
            {
                Code = 123,
                Message = Guid.NewGuid().ToString()
            };

            var sut = new ErrorObjectResult(error);

            sut.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            sut.Value.Should().Be(error);
        }

        [Fact]
        public void BuildsErrorWithShieldMessageWhenNoObjectOrMessageSpecifiedTest()
        {
            var statusCode = HttpStatusCode.BadRequest;

            var sut = new ErrorObjectResult(statusCode);

            sut.StatusCode.Should().Be((int)statusCode);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void BuildsErrorWithSpecifiedMessageAndStatusCodeTest()
        {
            var message = Guid.NewGuid().ToString();
            var statusCode = HttpStatusCode.BadRequest;

            var sut = new ErrorObjectResult(message, statusCode);

            sut.StatusCode.Should().Be((int)statusCode);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().Be(message);
        }

        [Fact]
        public void BuildsErrorWithSpecifiedObjectAndStatusCodeTest()
        {
            var error = new
            {
                Code = 123,
                Message = Guid.NewGuid().ToString()
            };
            var statusCode = HttpStatusCode.BadRequest;

            var sut = new ErrorObjectResult(error, statusCode);

            sut.StatusCode.Should().Be((int)statusCode);
            sut.Value.Should().Be(error);
        }

        [Fact]
        public void CreatesWithInternalServerErrorAndShieldMessageWhenNothingSpecifiedTest()
        {
            var sut = new ErrorObjectResult();

            sut.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CreatesWithShieldMessageWhenNoMessageProvidedTest(string message)
        {
            var sut = new ErrorObjectResult(message);

            sut.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().NotBeNullOrWhiteSpace();
        }
    }
}