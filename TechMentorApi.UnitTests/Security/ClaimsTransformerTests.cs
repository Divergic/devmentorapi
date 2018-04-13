namespace TechMentorApi.UnitTests.Security
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Business;
    using FluentAssertions;
    using Model;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Security;
    using Xunit;
    using Xunit.Abstractions;

    public class ClaimsTransformerTests
    {
        private readonly ITestOutputHelper _output;

        public ClaimsTransformerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullLoggerTest()
        {
            var manager = Substitute.For<IAccountQuery>();

            Action action = () => new ClaimsTransformer(manager, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullManagerTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();

            Action action = () => new ClaimsTransformer(null, logger);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task TransformAsyncAddsProfileIdClaimFromStoreToIdentityTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);
            var account = Model.Create<Account>();

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));

            var manager = Substitute.For<IAccountQuery>();

            manager.GetAccount(Arg.Is<User>(x => x.Username == identity.Name), Arg.Any<CancellationToken>())
                .Returns(account);

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            var expected = principal.Identity.As<ClaimsIdentity>().GetClaimValue<string>(ClaimType.ProfileId);

            expected.Should().Be(account.Id.ToString());
        }

        [Fact]
        public async Task TransformAsyncContinuesWhenAuth0ClaimNotFoundTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);
            var account = Model.Create<Account>();

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));

            var manager = Substitute.For<IAccountQuery>();

            manager.GetAccount(Arg.Is<User>(x => x.Username == identity.Name), Arg.Any<CancellationToken>())
                .Returns(account);

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            identity.Claims.Should().NotContain(x => x.Type == ClaimType.Role);
        }

        [Fact]
        public async Task TransformAsyncDoesNotAddProfileIdClaimWhenAlreadyPresentTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var claims = new[]
            {
                new Claim(ClaimType.Subject, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
                new Claim(ClaimType.ProfileId, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);

            var manager = Substitute.For<IAccountQuery>();

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            var expected = principal.Identity.As<ClaimsIdentity>().Claims.Where(x => x.Type == ClaimType.ProfileId);

            expected.Should().HaveCount(1);
            await manager.DidNotReceive().GetAccount(Arg.Any<User>(), CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TransformAsyncDoesNotAddProfileIdClaimWhenStoreReturnsNullAccountTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));

            var manager = Substitute.For<IAccountQuery>();

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            principal.HasClaim(x => x.Type == ClaimType.ProfileId).Should().BeFalse();
        }

        [Fact]
        public async Task TransformAsyncMapsAuth0RoleClaimToStandardRoleClaimTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);
            var account = Model.Create<Account>();

            var expectedRole = Guid.NewGuid().ToString();

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(ClaimType.Auth0Roles, expectedRole));

            var manager = Substitute.For<IAccountQuery>();

            manager.GetAccount(Arg.Is<User>(x => x.Username == identity.Name), Arg.Any<CancellationToken>())
                .Returns(account);

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            var expected = identity.GetClaimValue<string>(ClaimType.Role);

            expected.Should().Be(expectedRole);
            identity.Claims.Should().NotContain(x => x.Type == ClaimType.Auth0Roles);
        }

        [Fact]
        public async Task TransformAsyncProvidesAdditionalClaimsToManagerWhenGettingAccountTest()
        {
            var email = Guid.NewGuid().ToString();
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(ClaimType.Email, email));
            identity.AddClaim(new Claim(ClaimType.GivenName, firstName));
            identity.AddClaim(new Claim(ClaimType.Surname, lastName));

            var principal = new ClaimsPrincipal(identity);
            var account = Model.Create<Account>();
            
            var manager = Substitute.For<IAccountQuery>();

            manager.GetAccount(Arg.Is<User>(x => x.Username == identity.Name), Arg.Any<CancellationToken>())
                .Returns(account);

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            await manager.Received(1).GetAccount(Arg.Any<User>(), CancellationToken.None).ConfigureAwait(false);
            await manager.Received().GetAccount(Arg.Is<User>(x => x.Email == email), CancellationToken.None)
                .ConfigureAwait(false);
            await manager.Received().GetAccount(Arg.Is<User>(x => x.FirstName == firstName), CancellationToken.None)
                .ConfigureAwait(false);
            await manager.Received().GetAccount(Arg.Is<User>(x => x.LastName == lastName), CancellationToken.None)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TransformAsyncSkipsProcessingWhenUserNotAuthenticatedTest()
        {
            var logger = _output.BuildLoggerFor<ClaimsTransformer>();
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            var manager = Substitute.For<IAccountQuery>();

            var target = new ClaimsTransformer(manager, logger);

            await target.TransformAsync(principal).ConfigureAwait(false);

            await manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }
    }
}