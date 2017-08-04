namespace DevMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using Xunit;

    public class CacheConfigTests
    {
        [Theory]
        [InlineData(0, 300)]
        [InlineData(120, 120)]
        public void AccountExpirationReturnsConfigurationValueOrDefaultTest(int configValue, int expected)
        {
            var sut = new CacheConfig
            {
                AccountExpirationInSeconds = configValue
            };

            var actual = sut.AccountExpiration.TotalSeconds;

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 300)]
        [InlineData(120, 120)]
        public void CategoriesExpirationReturnsConfigurationValueOrDefaultTest(int configValue, int expected)
        {
            var sut = new CacheConfig
            {
                CategoriesExpirationInSeconds = configValue
            };

            var actual = sut.CategoriesExpiration.TotalSeconds;

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 300)]
        [InlineData(120, 120)]
        public void ProfileExpirationReturnsConfigurationValueOrDefaultTest(int configValue, int expected)
        {
            var sut = new CacheConfig
            {
                ProfileExpirationInSeconds = configValue
            };

            var actual = sut.ProfileExpiration.TotalSeconds;

            actual.Should().Be(expected);
        }
    }
}