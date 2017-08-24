namespace TechMentorApi.UnitTests.Security
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Business;
    using TechMentorApi.Model;
    using TechMentorApi.Security;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;
    using Xunit.Abstractions;

    public class AccountContextMiddlewareTests
    {
        private readonly ITestOutputHelper _output;

        public AccountContextMiddlewareTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InvokeAddsProfileIdClaimFromStoreToIdentityTest()
        {
            var delegateInvoked = false;
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var identity = new ClaimsIdentity("Bearer", ClaimType.Subject, ClaimType.Role);
            var principal = new ClaimsPrincipal(identity);
            var account = Model.Create<Account>();

            identity.AddClaim(new Claim(ClaimType.Subject, Guid.NewGuid().ToString()));

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);
            context.User = principal;

            manager.GetAccount(Arg.Is<User>(x => x.Username == identity.Name), Arg.Any<CancellationToken>())
                .Returns(account);

            var target = new AccountContextMiddleware(next);

            await target.Invoke(context, manager, logger).ConfigureAwait(false);

            var expected = principal.Identity.As<ClaimsIdentity>().GetClaimValue<string>(ClaimType.ProfileId);

            expected.Should().Be(account.Id.ToString());

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeDoesNotAddProfileIdClaimWhenAlreadyPresentTest()
        {
            var delegateInvoked = false;
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
                new Claim(ClaimType.ProfileId, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);
            context.User = principal;

            var target = new AccountContextMiddleware(next);

            await target.Invoke(context, manager, logger).ConfigureAwait(false);

            var expected = principal.Identity.As<ClaimsIdentity>().Claims.Where(x => x.Type == ClaimType.ProfileId);

            expected.Should().HaveCount(1);

            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeDoesNotExecuteHandlerForOptionsRequestTest()
        {
            var delegateInvoked = false;
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "OPTIONS";
            context.Request.Returns(request);

            var target = new AccountContextMiddleware(next);

            await target.Invoke(context, manager, logger).ConfigureAwait(false);

            await manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeSkipsProcessingWhenUserNotAuthenticatedTest()
        {
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);
            context.User = principal;

            var target = new AccountContextMiddleware(next);

            await target.Invoke(context, manager, logger).ConfigureAwait(false);

            await manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeSkipsProcessingWithNullUserTest()
        {
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();

            request.Method = "GET";
            context.Request.Returns(request);

            var target = new AccountContextMiddleware(next);

            await target.Invoke(context, manager, logger).ConfigureAwait(false);

            await manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
            delegateInvoked.Should().BeTrue();
        }

        [Fact]
        public void InvokeThrowsExceptionWhenCreatedWithNullContextTest()
        {
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var manager = Substitute.For<IAccountManager>();

            var target = new AccountContextMiddleware(next);

            Func<Task> action = async () => await target.Invoke(null, manager, logger).ConfigureAwait(false);

            manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>());
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

            var manager = Substitute.For<IAccountManager>();
            var context = Substitute.For<HttpContext>();

            var target = new AccountContextMiddleware(next);

            Func<Task> action = async () => await target.Invoke(context, manager, null).ConfigureAwait(false);

            manager.DidNotReceive().GetAccount(Arg.Any<User>(), Arg.Any<CancellationToken>());
            delegateInvoked.Should().BeFalse();

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void InvokeThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var logger = _output.BuildLoggerFor<AccountContextMiddleware>();
            var delegateInvoked = false;

            RequestDelegate next = delegate
            {
                delegateInvoked = true;

                return Task.CompletedTask;
            };

            var context = Substitute.For<HttpContext>();

            var target = new AccountContextMiddleware(next);

            Func<Task> action = async () => await target.Invoke(context, null, logger).ConfigureAwait(false);

            delegateInvoked.Should().BeFalse();

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullDelegateTest()
        {
            Action action = () => new AccountContextMiddleware(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}