namespace DevMentorApi.UnitTests.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Controllers;
    using DevMentorApi.Core;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class AccountProfileControllerTests
    {
        [Fact]
        public async Task GetReturnsNotFoundWhenManagerReturnsNullTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var manager = Substitute.For<IProfileManager>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task GetReturnsProfileForSpecifiedIdTest()
        {
            var account = Model.Create<Account>();
            var profile = Model.Create<Profile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var manager = Substitute.For<IProfileManager>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetProfile(account.Id, tokenSource.Token).Returns(profile);

                using (var target = new AccountProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();

                    result.Value.ShouldBeEquivalentTo(profile);
                }
            }
        }

        [Fact]
        public async Task PutProvidesProfileToManagerTest()
        {
            var account = Model.Create<Account>();
            var expected = Model.Create<UpdatableProfile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var manager = Substitute.For<IProfileManager>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Put(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<NoContentResult>();

                    await manager.Received().UpdateProfile(account.Id, expected, tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task PutReturnsBadRequestWithNoPutDataTest()
        {
            var manager = Substitute.For<IProfileManager>();

            using (var target = new AccountProfileController(manager))
            {
                var actual = await target.Put(null, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.BadRequest);
            }
        }
        
        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullManagerTest()
        {
            Action action = () => new AccountProfileController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}