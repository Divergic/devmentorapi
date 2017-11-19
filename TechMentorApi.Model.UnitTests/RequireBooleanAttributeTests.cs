namespace TechMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using TechMentorApi.Model;
    using Xunit;

    public class RequireBooleanAttributeTests
    {
        [Fact]
        public void FormatMessageIncludesFieldNameTest()
        {
            var name = Guid.NewGuid().ToString();

            var sut = new RequireBooleanAttribute();

            var actual = sut.FormatErrorMessage(name);

            actual.Should().Contain(name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsValidReturnsExpectedValueTest(bool expected)
        {
            var sut = new RequireBooleanAttribute();

            var actual = sut.IsValid(expected);

            actual.Should().Be(expected);
        }

        [Fact]
        public void IsValidReturnsFalseWithIncorrectValueTypeTest()
        {
            var sut = new RequireBooleanAttribute();

            var actual = sut.IsValid(Guid.NewGuid());

            actual.Should().BeFalse();
        }

        [Fact]
        public void IsValidReturnsFalseWithNullValueTest()
        {
            var sut = new RequireBooleanAttribute();

            var actual = sut.IsValid(null);

            actual.Should().BeFalse();
        }
    }
}