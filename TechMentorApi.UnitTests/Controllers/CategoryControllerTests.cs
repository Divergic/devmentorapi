namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
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
    using TechMentorApi.Security;
    using TechMentorApi.ViewModels;
    using Xunit;

    public class CategoryControllerTests
    {
        [Fact]
        public async Task GetReturnsCategoryWhenUserIsAdministratorTest()
        {
            var claims = new List<Claim>
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("role", Role.Administrator)
            };
            var identity = new ClaimsIdentity(claims, "testing", "sub", "role");
            var principal = new ClaimsPrincipal(identity);
            var category = Model.Create<Category>();

            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategory(ReadType.All, category.Group, category.Name, tokenSource.Token).Returns(category);

                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(category.Group.ToString(), category.Name, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as Category;

                    resultValues.Should().BeEquivalentTo(category);
                }
            }
        }

        [Theory]
        [InlineData(null, "stuff")]
        [InlineData("", "stuff")]
        [InlineData("  ", "stuff")]
        [InlineData("other", "stuff")]
        [InlineData("skill", null)]
        [InlineData("skill", "")]
        [InlineData("skill", "  ")]
        public async Task GetReturnsNotFoundWhenGroupOrNameAreInvalidTest(string group, string name)
        {
            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = null;

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(group, name, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                        .Be((int)HttpStatusCode.NotFound);
                }
            }
        }

        [Fact]
        public async Task GetReturnsNotFoundWhenQueryReturnsNullTest()
        {
            var category = Model.Create<Category>();

            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = null;

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(category.Group.ToString(), category.Name, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                        .Be((int)HttpStatusCode.NotFound);
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoryWhenUserIsNullTest()
        {
            var category = Model.Create<Category>();

            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = null;

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategory(ReadType.VisibleOnly, category.Group, category.Name, tokenSource.Token)
                    .Returns(category);

                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(category.Group.ToString(), category.Name, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as PublicCategory;

                    resultValues.Should().BeEquivalentTo(category, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoryWhenUserNotAdministratorTest()
        {
            var claims = new List<Claim>
            {
                new Claim("sub", Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "testing", "sub", "role");
            var principal = new ClaimsPrincipal(identity);
            var category = Model.Create<Category>();

            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategory(ReadType.VisibleOnly, category.Group, category.Name, tokenSource.Token)
                    .Returns(category);

                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(category.Group.ToString(), category.Name, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as PublicCategory;

                    resultValues.Should().BeEquivalentTo(category, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task GetReturnsPublicCategoryWhenUserNotAuthenticatedTest()
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            var category = Model.Create<Category>();

            var query = Substitute.For<ICategoryQuery>();
            var command = Substitute.For<ICategoryCommand>();
            var httpContext = Substitute.For<HttpContext>();

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            httpContext.User = principal;

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategory(ReadType.VisibleOnly, category.Group, category.Name, tokenSource.Token)
                    .Returns(category);

                using (var target = new CategoryController(query, command))
                {
                    target.ControllerContext = controllerContext;

                    var actual = await target.Get(category.Group.ToString(), category.Name, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<OkObjectResult>();

                    var result = actual.As<OkObjectResult>();
                    var resultValues = result.Value as PublicCategory;

                    resultValues.Should().BeEquivalentTo(category, opt => opt.ExcludingMissingMembers());
                }
            }
        }

        [Fact]
        public async Task PutProvidesCategoryToManagerTest()
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();
            var query = Substitute.For<ICategoryQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(query, command))
                {
                    var actual = await target.Put(group.ToString(), name, model, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<StatusCodeResult>();

                    var result = actual.As<StatusCodeResult>();

                    result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

                    await command.Received(1).UpdateCategory(Arg.Any<Category>(), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.Group == group), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.Name == name), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(
                        Arg.Is<Category>(x => x.Visible == model.Visible),
                        tokenSource.Token).ConfigureAwait(false);
                    await command.Received().UpdateCategory(
                        Arg.Is<Category>(x => x.Reviewed == false),
                        tokenSource.Token).ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.LinkCount == 0), tokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
        }

        [Theory]
        [InlineData("gender")]
        [InlineData("Gender")]
        [InlineData("GENDER")]
        public async Task PutProvidesCategoryToManagerWithCaseInsensitiveCategoryGroupMatchTest(string group)
        {
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();
            var query = Substitute.For<ICategoryQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(query, command))
                {
                    var actual = await target.Put(group, name, model, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<StatusCodeResult>();

                    var result = actual.As<StatusCodeResult>();

                    result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

                    await command.Received().UpdateCategory(
                        Arg.Is<Category>(x => x.Group == CategoryGroup.Gender),
                        tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("stuff")]
        [InlineData(" ")]
        public async Task PutReturnsBadRequestWithInvalidGroupTest(string group)
        {
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();
            var query = Substitute.For<ICategoryQuery>();

            using (var target = new CategoryController(query, command))
            {
                var actual = await target.Put(group, name, model, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.NotFound);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task PutReturnsBadRequestWithInvalidNameTest(string name)
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();
            var query = Substitute.For<ICategoryQuery>();

            using (var target = new CategoryController(query, command))
            {
                var actual = await target.Put(group.ToString(), name, model, CancellationToken.None)
                    .ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task PutReturnsBadRequestWithNoPutDataTest()
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var name = Guid.NewGuid().ToString();

            var command = Substitute.For<ICategoryCommand>();
            var query = Substitute.For<ICategoryQuery>();

            using (var target = new CategoryController(query, command))
            {
                var actual = await target.Put(group.ToString(), name, null, CancellationToken.None)
                    .ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int)HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCommandTest()
        {
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new CategoryController(query, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var command = Substitute.For<ICategoryCommand>();

            Action action = () => new CategoryController(null, command);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}