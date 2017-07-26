namespace DevMentorApi.Model.UnitTests
{
    using System;
    using FluentAssertions;
    using ModelBuilder;
    using Xunit;

    public class AccountTests
    {
        [Fact]
        public void CanCreateNewInstanceTest()
        {
            var sut = new Account();

            sut.BannedAt.Should().NotHaveValue();
            sut.Id.Should().BeEmpty();
            sut.Provider.Should().BeNull();
            sut.Username.Should().BeNull();
        }

        [Fact]
        public void CopyConstructorCopiesAllBaseValuesTest()
        {
            var source = Model.Create<NewAccount>();

            var sut = new Account(source);

            sut.ShouldBeEquivalentTo(source, opt => opt.Excluding(x => x.BannedAt));
            sut.BannedAt.Should().NotHaveValue();
        }

        [Fact]
        public void CopyConstructorThrowsExceptionWithNullAccountTest()
        {
            Action action = () => new Account(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}