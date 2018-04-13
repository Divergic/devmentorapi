namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using FluentAssertions;
    using ModelBuilder;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountResultTests
    {
        [Fact]
        public void CopiesValuesFromSourceAccountTest()
        {
            var account = Model.Create<Account>();

            var sut = new AccountResult(account);

            sut.Should().BeEquivalentTo(account, opt => opt.ExcludingMissingMembers());
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullSourceTest()
        {
            Action action = () => new AccountResult(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}