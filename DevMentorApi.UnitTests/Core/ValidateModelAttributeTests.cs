namespace DevMentorApi.UnitTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using NSubstitute;
    using DevMentorApi.Core;
    using Xunit;

    public class ValidateModelAttributeTests
    {
        [Fact]
        public void OnActionExecutingHasNoEffectWhenModelStateIsValidTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var arguments = new Dictionary<string, object>();
            var executingContext = new ActionExecutingContext(actionContext, filters, arguments, null);
            var request = Substitute.For<HttpRequest>();

            request.Method = HttpMethod.Post.ToString();

            httpContext.Request.Returns(request);

            var target = new ValidateModelAttribute();

            target.OnActionExecuting(executingContext);

            executingContext.Result.Should().BeNull();
        }

        [Fact]
        public void OnActionExecutingHasNoEffectWhenRequestIsGetTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var arguments = new Dictionary<string, object>();
            var executingContext = new ActionExecutingContext(actionContext, filters, arguments, null);
            var request = Substitute.For<HttpRequest>();
            var context = Substitute.For<HttpContext>();

            request.Method.Returns(HttpMethod.Get.ToString());
            context.Request.Returns(request);

            executingContext.HttpContext = context;
            executingContext.ModelState.AddModelError(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var target = new ValidateModelAttribute();

            target.OnActionExecuting(executingContext);

            executingContext.Result.Should().BeNull();
        }

        [Fact]
        public void OnActionExecutingHasNoEffectWhenRequestIsHeadTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var arguments = new Dictionary<string, object>();
            var executingContext = new ActionExecutingContext(actionContext, filters, arguments, null);
            var request = Substitute.For<HttpRequest>();
            var context = Substitute.For<HttpContext>();

            request.Method = HttpMethod.Head.ToString();
            context.Request.Returns(request);

            executingContext.HttpContext = context;
            executingContext.ModelState.AddModelError(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var target = new ValidateModelAttribute();

            target.OnActionExecuting(executingContext);

            executingContext.Result.Should().BeNull();
        }

        [Fact]
        public void OnActionExecutingSetsBadRequestResponseWhenModelIsInvalidTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var arguments = new Dictionary<string, object>();
            var executingContext = new ActionExecutingContext(actionContext, filters, arguments, null);
            var request = Substitute.For<HttpRequest>();
            var context = Substitute.For<HttpContext>();

            request.Method = HttpMethod.Post.ToString();
            context.Request.Returns(request);

            executingContext.HttpContext = context;
            executingContext.ModelState.AddModelError(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var target = new ValidateModelAttribute();

            target.OnActionExecuting(executingContext);

            executingContext.Result.Should().NotBeNull();
            executingContext.Result.Should().BeOfType<BadRequestObjectResult>();
            executingContext.Result.As<BadRequestObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public void OnActionExecutingThrowsExceptionWithNullContextTest()
        {
            var target = new ValidateModelAttribute();

            Action action = () => target.OnActionExecuting(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}