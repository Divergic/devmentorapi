namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class ProfileChangeCalculatorTests
    {
        [Fact]
        public void CalculateChangesReturnsNoChangesWhenProfilesMatchTest()
        {
            var original = Model.Create<Profile>();
            var updated = original.Clone();

            var sut = new ProfileChangeCalculator();

            var actual = sut.CalculateChanges(original, updated);

            actual.ProfileChanged.Should().BeFalse();
            actual.CategoryChanges.Should().BeEmpty();
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWhenProfileIsNotMatchingTest()
        {
            var original = Model.Create<Profile>();
            var updated = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(original, updated);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWithNullOriginalProfileTest()
        {
            var updated = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(null, updated);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CalculateChangesThrowsExceptionWithNullUpdatedProfileTest()
        {
            var original = Model.Create<Profile>();

            var sut = new ProfileChangeCalculator();

            Action action = () => sut.CalculateChanges(original, null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}