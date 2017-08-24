namespace TechMentorApi.UnitTests
{
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using FluentAssertions;
    using Xunit;

    public class IdentityExtensionsTests
    {
        [Fact]
        public void GetClaimValueReturnsDefaultValueWhenIdentityIsNotClaimsIdentityTest()
        {
            IIdentity identity = new GenericIdentity(Guid.NewGuid().ToString());

            var actual = identity.GetClaimValue<Guid>("Something");

            actual.Should().Be(Guid.Empty);
        }

        [Fact]
        public void GetClaimValueReturnsDefaultValueWhenIdentityIsNullTest()
        {
            IIdentity identity = null;

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

            IIdentity target = source;

            var actual = target.GetClaimValue<string>(claimType);

            actual.Should().Be(expected);
        }
    }
}