namespace TechMentorApi.Model.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using FluentAssertions;
    using Xunit;

    public class NoSlashAttributeTests
    {
        [Fact]
        public void FormatMessageIncludesFieldNameTest()
        {
            var name = Guid.NewGuid().ToString();

            var sut = new NoSlashAttribute();

            var actual = sut.FormatErrorMessage(name);

            actual.Should().Contain(name);
        }

        [Fact]
        public void IsValidReturnsTrueWithNullValueTest()
        {
            var sut = new NoSlashAttribute();

            var actual = sut.IsValid(null);

            actual.Should().BeTrue();
        }

        [Fact]
        public void IsValidReturnsTrueWithUnsupportedValueTypeTest()
        {
            var sut = new NoSlashAttribute();

            var actual = sut.IsValid(Guid.NewGuid());

            actual.Should().BeTrue();
        }

        [Theory]
        [InlineData("something", true)]
        [InlineData("some/here", false)]
        [InlineData("/somehere", false)]
        [InlineData("somehere/", false)]
        [InlineData("some\\here", false)]
        [InlineData("\\somehere", false)]
        [InlineData("somehere\\", false)]
        public void IsValidReturnsWhetherCollectionStringIsValidTest(string value, bool expected)
        {
            var items = new Collection<string>
            {
                "first",
                value,
                "second"
            };
            var sut = new NoSlashAttribute();

            var actual = sut.IsValid(items);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("something", true)]
        [InlineData("some/here", false)]
        [InlineData("/somehere", false)]
        [InlineData("somehere/", false)]
        [InlineData("some\\here", false)]
        [InlineData("\\somehere", false)]
        [InlineData("somehere\\", false)]
        public void IsValidReturnsWhetherListStringIsValidTest(string value, bool expected)
        {
            var items = new List<string>
            {
                "first",
                value,
                "second"
            };
            var sut = new NoSlashAttribute();

            var actual = sut.IsValid(items);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("something", true)]
        [InlineData("some/here", false)]
        [InlineData("/somehere", false)]
        [InlineData("somehere/", false)]
        [InlineData("some\\here", false)]
        [InlineData("\\somehere", false)]
        [InlineData("somehere\\", false)]
        public void IsValidReturnsWhetherStringIsValidTest(string value, bool expected)
        {
            var sut = new NoSlashAttribute();

            var actual = sut.IsValid(value);

            actual.Should().Be(expected);
        }
    }
}