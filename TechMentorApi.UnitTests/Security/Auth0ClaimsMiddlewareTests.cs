namespace TechMentorApi.UnitTests.Security
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Model;
    using NSubstitute;
    using TechMentorApi.Security;
    using Xunit;
    using Xunit.Abstractions;

    public class Auth0ClaimsMiddlewareTests
    {
        private readonly ITestOutputHelper _output;

        public Auth0ClaimsMiddlewareTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InvokeDoesNotExecuteHandlerForOptionsRequestTest()
        {
            var delegateInvoked = false;
            var logger = _output.BuildLoggerFor<Auth0ClaimsMiddleware>();

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "OPTIONS";
            context.Request.Returns(request);

            var target = new Auth0ClaimsMiddleware(next);

            await target.Invoke(context, logger).ConfigureAwait(false);

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeMapsAuth0RoleClaimToStandardRoleClaimTest()
        {
            var delegateInvoked = false;
            var logger = _output.BuildLoggerFor<Auth0ClaimsMiddleware>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);

            var expectedRole = Guid.NewGuid().ToString();

            identity.AddClaim(new Claim(ClaimType.Auth0Roles, expectedRole));

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);
            context.User = principal;

            var target = new Auth0ClaimsMiddleware(next);

            await target.Invoke(context, logger).ConfigureAwait(false);

            var expected = identity.GetClaimValue<string>(ClaimType.Role);

            expected.Should().Be(expectedRole);
            identity.Claims.Should().NotContain(x => x.Type == ClaimType.Auth0Roles);

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeSkipsProcessingWhenUserNotAuthenticatedTest()
        {
            var logger = _output.BuildLoggerFor<Auth0ClaimsMiddleware>();
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);
            context.User = principal;

            var target = new Auth0ClaimsMiddleware(next);

            await target.Invoke(context, logger).ConfigureAwait(false);

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeSkipsProcessingWithNullUserTest()
        {
            var logger = _output.BuildLoggerFor<Auth0ClaimsMiddleware>();
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);

            var target = new Auth0ClaimsMiddleware(next);

            await target.Invoke(context, logger).ConfigureAwait(false);

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public void InvokeThrowsExceptionWhenCreatedWithNullContextTest()
        {
            var logger = _output.BuildLoggerFor<Auth0ClaimsMiddleware>();
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var target = new Auth0ClaimsMiddleware(next);

            Func<Task> action = async () => await target.Invoke(null, logger).ConfigureAwait(false);

            delegateInvoked.Should().BeFalse();

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void InvokeThrowsExceptionWhenCreatedWithNullLoggerTest()
        {
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();

            var target = new Auth0ClaimsMiddleware(next);

            Func<Task> action = async () => await target.Invoke(context, null).ConfigureAwait(false);

            delegateInvoked.Should().BeFalse();

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullDelegateTest()
        {
            Action action = () => new Auth0ClaimsMiddleware(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}