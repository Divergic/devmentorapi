namespace DevMentorApi.UnitTests.ViewModels
{
    using System;
    using DevMentorApi.Model;
    using DevMentorApi.ViewModels;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class PublicCategoryTests
    {
        [Fact]
        public void CanCreateDefaultInstanceTest()
        {
            var sut = new PublicCategory();

            sut.Group.Should().Be(CategoryGroup.Skill);
            sut.LinkCount.Should().Be(0);
            sut.Name.Should().BeNull();
        }

        [Fact]
        public void CategoryConstructorReturnsMatchingInformationTest()
        {
            var category = Model.Create<Category>();

            var sut = new PublicCategory(category);

            sut.ShouldBeEquivalentTo(category, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void CategoryConstructorThrowsExceptionWithNullCategoryTest()
        {
            Action action = () => new PublicCategory(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}