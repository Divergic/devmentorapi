namespace DevMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class ValidPastYearAttributeTests
    {
        [Fact]
        public void CanCreateWithCurrentYearTest()
        {
            Action action = () => new ValidPastYearAttribute(DateTimeOffset.UtcNow.Year);

            action.ShouldNotThrow();
        }

        [Theory]
        [InlineData(2017, null, false, false)]
        [InlineData(2017, null, true, true)]
        [InlineData(2017, "asdf", false, false)]
        [InlineData(2017, 2016, false, false)]
        [InlineData(2016, 3000, false, false)]
        [InlineData(2015, 2016, false, true)]
        [InlineData(2016, 2016, false, true)]
        public void IsValidReturnsExpectedValueTest(int minYear, object value, bool allowNull, bool expected)
        {
            var sut = new ValidPastYearAttribute(minYear, allowNull);

            var actual = sut.IsValid(value);

            actual.Should().Be(expected);
        }

        [Fact]
        public void IsValidReturnsTrueForCurrentYearTest()
        {
            var year = DateTimeOffset.UtcNow.Year;

            var sut = new ValidPastYearAttribute(year);

            var actual = sut.IsValid(year);

            actual.Should().BeTrue();
        }

        [Fact]
        public void ThrowsExceptionWithYearGreaterThanCurrentYearTest()
        {
            Action action = () => new ValidPastYearAttribute(DateTimeOffset.UtcNow.Year + 1);

            action.ShouldThrow<ArgumentException>();
        }
    }
}