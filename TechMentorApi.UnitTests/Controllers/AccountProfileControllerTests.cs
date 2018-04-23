namespace TechMentorApi.UnitTests.Controllers
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using ModelBuilder;
    using NSubstitute;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountProfileControllerTests
    {
        [Fact]
        public async Task DeleteReturnsNoContentTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Create<Profile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Delete(tokenSource.Token).ConfigureAwait(false);

                    await accountCommand.Received().DeleteAccount(user.Identity.Name, account.Id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<NoContentResult>();
                }
            }
        }

        [Fact]
        public async Task GetAttemptsToGetProfileMultipleTimesTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Create<Profile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileQuery.GetProfile(account.Id, tokenSource.Token).Returns(null, null, null, profile);

                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();

                    result.Value.Should().BeEquivalentTo(profile);

                    await profileQuery.Received(4).GetProfile(account.Id, tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task GetReturnsNotFoundAfterAllRetryAttemptsFailTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

                    await profileQuery.Received(4).GetProfile(account.Id, tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task GetReturnsNotFoundWhenQueryReturnsNullTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
                }
            }
        }

        [Fact]
        public async Task GetReturnsProfileForSpecifiedIdTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Create<Profile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileQuery.GetProfile(account.Id, tokenSource.Token).Returns(profile);

                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();

                    result.Value.Should().BeEquivalentTo(profile);
                }
            }
        }

        [Fact]
        public async Task PutProvidesProfileToManagerTest()
        {
            var account = Model.Create<Account>();
            var expected = Model.Create<UpdatableProfile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Put(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<NoContentResult>();

                    await profileCommand.Received().UpdateProfile(account.Id, expected, tokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task PutReturnsBadRequestWithNoPutDataTest()
        {
            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();

            using (var target = new AccountProfileController(profileQuery, profileCommand, accountCommand))
            {
                var actual = await target.Put(null, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullAccountCommandTest()
        {
            var profileQuery = Substitute.For<IProfileQuery>();
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();

            Action action = () => new AccountProfileController(profileQuery, profileCommand, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileCommandTest()
        {
            var profileQuery = Substitute.For<IProfileQuery>();
            var accountCommand = Substitute.For<IAccountCommand>();

            Action action = () => new AccountProfileController(profileQuery, null, accountCommand);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileQueryTest()
        {
            var profileCommand = Substitute.For<IProfileCommand>();
            var accountCommand = Substitute.For<IAccountCommand>();

            Action action = () => new AccountProfileController(null, profileCommand, accountCommand);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}