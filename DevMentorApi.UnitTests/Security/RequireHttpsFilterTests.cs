namespace DevMentorApi.UnitTests.Security
{
    using System.Collections.Generic;
    using System.Net;
    using DevMentorApi.Core;
    using DevMentorApi.Security;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using NSubstitute;
    using Xunit;

    public class RequireHttpsFilterTests
    {
        [Fact]
        public void OnActionExecutedDoesNotSetResultTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var context = new ActionExecutedContext(actionContext, filters, null);

            httpContext.Request.Returns(request);
            request.IsHttps.Returns(true);

            var target = new RequireHttpsFilter();

            target.OnActionExecuted(context);

            context.Result.Should().BeNull();
        }

        [Fact]
        public void OnActionExecutingDoesNotSetResultWhenRequestIsSecureTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var actionArguments = new Dictionary<string, object>();
            var context = new ActionExecutingContext(actionContext, filters, actionArguments, null);

            httpContext.Request.Returns(request);
            request.IsHttps.Returns(true);

            var target = new RequireHttpsFilter();

            target.OnActionExecuting(context);

            context.Result.Should().BeNull();
        }

        [Fact]
        public void OnActionExecutingSetsResultWhenRequestIsInsecureTest()
        {
            var httpContext = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var filters = new List<IFilterMetadata>();
            var actionArguments = new Dictionary<string, object>();
            var context = new ActionExecutingContext(actionContext, filters, actionArguments, null);

            httpContext.Request.Returns(request);
            request.IsHttps.Returns(false);

            var target = new RequireHttpsFilter();

            target.OnActionExecuting(context);

            context.Result.Should().BeOfType<ErrorObjectResult>();
            context.Result.As<ErrorObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }
    }
}