namespace TechMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class AccountTests
    {
        private readonly ITestOutputHelper _output;

        public AccountTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanCreateWithDefaultValuesTest()
        {
            var sut = new Account();

            sut.Id.Should().BeEmpty();
            sut.Provider.Should().BeNull();
            sut.Subject.Should().BeNull();
            sut.Username.Should().BeNull();
        }

        [Theory]
        [InlineData("username", "Unspecified", "username")]
        [InlineData("provider|username", "provider", "username")]
        public void CreateParsesProviderAndUsernameTest(string value, string provider, string username)
        {
            _output.WriteLine("Testing with {0}", value);

            var sut = new Account(value);

            sut.Id.Should().BeEmpty();
            sut.Provider.Should().Be(provider);
            sut.Subject.Should().Be(username);
            sut.Username.Should().Be(sut.Provider + "|" + sut.Subject);
        }
    }
}