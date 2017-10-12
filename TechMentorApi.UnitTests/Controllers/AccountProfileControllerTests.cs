namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using Xunit;

    public class AccountProfileControllerTests
    {
        [Fact]
        public async Task GetReturnsNotFoundWhenManagerReturnsNullTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(query, command))
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

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetProfile(account.Id, tokenSource.Token).Returns(profile);

                using (var target = new AccountProfileController(query, command))
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

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AccountProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Put(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<NoContentResult>();

                    await command.Received().UpdateProfile(account.Id, expected, tokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task PutReturnsBadRequestWithNoPutDataTest()
        {
            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();

            using (var target = new AccountProfileController(query, command))
            {
                var actual = await target.Put(null, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int) HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCommandTest()
        {
            var query = Substitute.For<IProfileQuery>();

            Action action = () => new AccountProfileController(query, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var command = Substitute.For<IProfileCommand>();

            Action action = () => new AccountProfileController(null, command);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}