namespace DevMentorApi.UnitTests
{
    using System;
    using System.Security.Claims;
    using FluentAssertions;
    using Xunit;

    public class ClaimsIdentityExtensionsTests
    {
        [Fact]
        public void GetClaimValueReturnsDefaultValueWhenIdentityDoesNotContainRequestedClaimsTest()
        {
            var identity = new ClaimsIdentity(Guid.NewGuid().ToString());

            var actual = identity.GetClaimValue<Guid>("Something");

            actual.Should().Be(Guid.Empty);
        }

        [Fact]
        public void GetClaimValueReturnsDefaultValueWhenIdentityIsNullTest()
        {
            ClaimsIdentity identity = null;

            var actual = identity.GetClaimValue<Guid>("Something");

            actual.Should().Be(Guid.Empty);
        }

        [Fact]
        public void GetClaimValueReturnsValidClaimValueTest()
        {
            var source = new ClaimsIdentity(Guid.NewGuid().ToString());
            var claimType = Guid.NewGuid().ToString();
            var expected = Guid.NewGuid().ToString();

            source.AddClaim(new Claim(claimType, expected));

            var target = source;

            var actual = target.GetClaimValue<string>(claimType);

            actual.Should().Be(expected);
        }

        [Fact]
        public void GetClaimValueReturnsValidGuidClaimValueTest()
        {
            var source = new ClaimsIdentity(Guid.NewGuid().ToString());
            var claimType = Guid.NewGuid().ToString();
            var expected = Guid.NewGuid();

            source.AddClaim(new Claim(claimType, expected.ToString()));

            var target = source;

            var actual = target.GetClaimValue<Guid>(claimType);

            actual.Should().Be(expected);
        }
    }
}