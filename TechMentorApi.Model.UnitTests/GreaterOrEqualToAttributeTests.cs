namespace TechMentorApi.Model.UnitTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;
    using Xunit.Abstractions;

    public class GreaterOrEqualToAttributeTests
    {
        private readonly ITestOutputHelper _output;

        public GreaterOrEqualToAttributeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void IsValidReturnsFailureWhenOtherValueIsGreaterTest()
        {
            var skill = Model.Create<Skill>().Set(x => x.YearStarted = 2000).Set(x => x.YearLastUsed = 1999);
            var context = new ValidationContext(skill)
            {
                MemberName = nameof(Skill.YearLastUsed)
            };
            
            var sut = new GreaterOrEqualToAttribute(nameof(Skill.YearStarted));

            var actual = sut.GetValidationResult(skill.YearLastUsed, context);

            _output.WriteLine(actual.ErrorMessage);

            actual.Should().NotBe(ValidationResult.Success);
            actual.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            actual.MemberNames.Should().Contain(nameof(Skill.YearStarted));
            actual.MemberNames.Should().Contain(nameof(Skill.YearLastUsed));
        }

        [Theory]
        [InlineData(1989, 1989)]
        [InlineData(1989, 2010)]
        public void IsValidReturnsSuccessWhenOtherValueIsLessOrEqualToTest(int yearStarted, int yearLastUsed)
        {
            var skill = Model.Create<Skill>().Set(x => x.YearStarted = yearStarted)
                .Set(x => x.YearLastUsed = yearLastUsed);
            var context = new ValidationContext(skill);

            var sut = new GreaterOrEqualToAttribute(nameof(Skill.YearStarted));

            var actual = sut.GetValidationResult(skill.YearLastUsed, context);

            actual.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValidReturnsSuccessWhenOtherValueIsNullTest()
        {
            var skill = Model.Create<Skill>().Set(x => x.YearStarted = null).Set(x => x.YearLastUsed = 2010);
            var context = new ValidationContext(skill);

            var sut = new GreaterOrEqualToAttribute(nameof(Skill.YearStarted));

            var actual = sut.GetValidationResult(skill.YearLastUsed, context);

            actual.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValidReturnsSuccessWhenValueIsNullTest()
        {
            var skill = Model.Create<Skill>();
            var context = new ValidationContext(skill);

            var sut = new GreaterOrEqualToAttribute(nameof(Skill.YearStarted));

            var actual = sut.GetValidationResult(null, context);

            actual.Should().Be(ValidationResult.Success);
        }
        
        [Fact]
        public void IsValidThrowsExceptionWhenOtherPropertyNotFoundTest()
        {
            var skill = Model.Create<Skill>().Set(x => x.YearLastUsed = 2015);
            var context = new ValidationContext(skill);

            var sut = new GreaterOrEqualToAttribute("NotHere");
            
            Action action = () => sut.GetValidationResult(skill.YearLastUsed, context);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithInvalidOtherPropertyNameTest()
        {
            Action action = () => new GreaterOrEqualToAttribute(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}