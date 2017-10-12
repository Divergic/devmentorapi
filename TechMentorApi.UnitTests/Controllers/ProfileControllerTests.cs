namespace TechMentorApi.UnitTests.Controllers
{
    using System;
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

    public class ProfileControllerTests
    {
        [Fact]
        public async Task DeleteBansProfileTest()
        {
            var profile = Model.Create<Profile>();

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    command.BanProfile(profile.Id, Arg.Any<DateTimeOffset>(), tokenSource.Token).Returns(profile);

                    var actual = await target.Delete(profile.Id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<NoContentResult>();

                    await command.Received().BanProfile(profile.Id,
                        Verify.That<DateTimeOffset>(x => x.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000)),
                        tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task DeleteReturnsNotFoundWhenManagerReturnsNullTest()
        {
            var id = Guid.NewGuid();

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Delete(id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task DeleteReturnsNotFoundWithEmptyIdTest()
        {
            var id = Guid.Empty;

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Delete(id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task GetReturnsNotFoundWhenManagerReturnsNullTest()
        {
            var id = Guid.NewGuid();

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(query, command))
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
            var id = Guid.Empty;

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new ProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        public async Task GetReturnsProfileForSpecifiedIdTest()
        {
            var profile = Model.Create<PublicProfile>();

            var query = Substitute.For<IProfileQuery>();
            var command = Substitute.For<IProfileCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetPublicProfile(profile.Id, tokenSource.Token).Returns(profile);

                using (var target = new ProfileController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(profile.Id, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();

                    result.Value.Should().Be(profile);
                }
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCommandTest()
        {
            var query = Substitute.For<IProfileQuery>();

            Action action = () => new ProfileController(query, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var command = Substitute.For<IProfileCommand>();

            Action action = () => new ProfileController(null, command);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}