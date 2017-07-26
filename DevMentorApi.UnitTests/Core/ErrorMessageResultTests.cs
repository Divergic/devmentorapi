namespace DevMentorApi.UnitTests.Core
{
    using System;
    using System.Net;
    using System.Reflection;
    using DevMentorApi.Core;
    using FluentAssertions;
    using Xunit;

    public class ErrorMessageResultTests
    {
        [Fact]
        public void BuildsResultWithMessageTest()
        {
            var message = Guid.NewGuid().ToString();
            var statusCode = HttpStatusCode.BadRequest;

            var sut = new ErrorMessageResult(message, statusCode);

            sut.StatusCode.Should().Be((int)statusCode);

            var value = sut.Value;

            // The internal value is a dynamic object so we need to provide a workaround to get access to the value via Reflection
            // Hacky, but acceptable for testing
            var actual = value.GetType().GetTypeInfo().GetProperty("Message", typeof(string)).GetValue(value) as string;

            actual.Should().Be(message);
        }
    }
}