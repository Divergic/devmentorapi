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
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using Xunit;

    public class ExportControllerTests
    {
        [Fact]
        public async Task GetReturnsNotFoundWhenQueryReturnsNullTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var query = Substitute.For<IExportQuery>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ExportController(query))
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
            var profile = Model.Create<ExportProfile>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var query = Substitute.For<IExportQuery>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetExportProfile(account.Id, tokenSource.Token).Returns(profile);

                using (var target = new ExportController(query))
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
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            Action action = () => new ExportController(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}