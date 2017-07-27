namespace DevMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using Xunit;

    public class AuthenticationConfigTests
    {
        [Theory]
        [InlineData(0, 300)]
        [InlineData(120, 120)]
        public void UserInfoCacheTtlReturnsConfigurationValueOrDefaultTest(int configValue, int expected)
        {
            var sut = new AuthenticationConfig
            {
                AccountCacheTimeoutInSeconds = configValue
            };

            var actual = sut.AccountCacheTtl.TotalSeconds;

            actual.Should().Be(expected);
        }
    }
}