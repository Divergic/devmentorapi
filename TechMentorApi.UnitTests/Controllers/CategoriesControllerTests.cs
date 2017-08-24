namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Business;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Security;
    using TechMentorApi.ViewModels;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class CategoriesControllerTests
    {
        [Fact]
        public async Task GetReturnsCategoriesWhenUserIsAdministratorTest()
        {
            var claims = new List<Claim>
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("role", Role.Administrator)
            };
            var identity = new ClaimsIdentity(claims, "testing", "sub", "role");
            var principal = new ClaimsPrincipal(identity);
            var categories = Model.Create<List<Category>>();

            var manager = Substitute.For<ICategoryManager>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetCategories(ReadType.All, tokenSource.Token).Returns(categories);

                using (var target = new CategoriesController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as IEnumerable<Category>;

                    resultValues.ShouldAllBeEquivalentTo(categories);
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoriesWhenUserIsNullTest()
        {
            var categories = Model.Create<List<Category>>();

            var manager = Substitute.For<ICategoryManager>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = null;

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                using (var target = new CategoriesController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as IEnumerable<PublicCategory>;

                    resultValues.ShouldAllBeEquivalentTo(categories, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoriesWhenUserNotAdministratorTest()
        {
            var claims = new List<Claim>
            {
                new Claim("sub", Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "testing", "sub", "role");
            var principal = new ClaimsPrincipal(identity);
            var categories = Model.Create<List<Category>>();

            var manager = Substitute.For<ICategoryManager>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                using (var target = new CategoriesController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as IEnumerable<PublicCategory>;

                    resultValues.ShouldAllBeEquivalentTo(categories, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoriesWhenUserNotAuthenticatedTest()
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            var categories = Model.Create<List<Category>>();

            var manager = Substitute.For<ICategoryManager>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                using (var target = new CategoriesController(manager))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as IEnumerable<PublicCategory>;

                    resultValues.ShouldAllBeEquivalentTo(categories, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task PostProvidesCategoryToManagerTest()
        {
            var expected = Model.Create<NewCategory>();

            var manager = Substitute.For<ICategoryManager>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoriesController(manager))
                {
                    var actual = await target.Post(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<StatusCodeResult>();

                    var result = actual.As<StatusCodeResult>();

                    result.StatusCode.Should().Be((int)HttpStatusCode.Created);

                    await manager.Received().CreateCategory(expected, tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task PostReturnsBadRequestWithNoPostDataTest()
        {
            var manager = Substitute.For<ICategoryManager>();

            using (var target = new CategoriesController(manager))
            {
                var actual = await target.Post(null, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullManagerTest()
        {
            Action action = () => new CategoriesController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}