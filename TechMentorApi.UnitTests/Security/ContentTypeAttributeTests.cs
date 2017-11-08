namespace TechMentorApi.UnitTests.Security
{
    using System;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using NSubstitute;
    using TechMentorApi.Security;
    using Xunit;

    public class ContentTypeAttributeTests
    {
        [Theory]
        [InlineData("image/jpeg", true)]
        [InlineData("image/png", true)]
        [InlineData("IMAGE/JPEG", true)]
        [InlineData("image/PNG", true)]
        [InlineData("Image/png", true)]
        [InlineData("image/gif", false)]
        [InlineData("application/octet-stream", false)]
        public void IsValidReturnsExpectedValueTest(string contentType, bool expected)
        {
            var file = Substitute.For<IFormFile>();

            file.ContentType.Returns(contentType);

            var sut = new ContentTypeAttribute();

            var actual = sut.IsValid(file);

            actual.Should().Be(expected);
        }

        [Fact]
        public void IsValidReturnsFalseWithIncorrectValueTypeTest()
        {
            var sut = new ContentTypeAttribute();

            var actual = sut.IsValid(Guid.NewGuid());

            actual.Should().BeFalse();
        }

        [Fact]
        public void IsValidReturnsFalseWithNullValueTest()
        {
            var sut = new ContentTypeAttribute();

            var actual = sut.IsValid(null);

            actual.Should().BeFalse();
        }
    }
}