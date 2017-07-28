namespace DevMentorApi.UnitTests.Controllers
{
    using System;
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

    public class ProfileControllerTests
    {
        [Fact]
        public async Task GetReturnsNotFoundWhenManagerReturnsNullTest()
        {
            var user = ClaimsIdentityFactory.BuildPrincipal();
            var id = Guid.NewGuid();

            var manager = Substitute.For<IProfileManager>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task GetReturnsNotFoundWithEmptyIdTest()
        {
            var user = ClaimsIdentityFactory.BuildPrincipal();
            var id = Guid.Empty;

            var manager = Substitute.For<IProfileManager>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task GetReturnsProfileForSpecifiedAccountIdTest()
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

                using (var target = new ProfileController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(account.Id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();

                    result.Value.ShouldBeEquivalentTo(profile, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullManagerTest()
        {
            Action action = () => new ProfileController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}