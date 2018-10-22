namespace TechMentorApi.UnitTests.Core
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using TechMentorApi.Core;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using Xunit;

    public class ResultExecutorTests
    {
        [Fact]
        public async Task ExecuteSendsResultToExecutorTest()
        {
            const HttpStatusCode StatusCode = HttpStatusCode.OK;
            var mvcOptions = new MvcOptions();
            var options = Substitute.For<IOptions<MvcOptions>>();

            mvcOptions.OutputFormatters.Add(new StringOutputFormatter());
            options.Value.Returns(mvcOptions);

            var writerFactory = Substitute.For<IHttpResponseStreamWriterFactory>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            
            var formatSelector = new DefaultOutputFormatterSelector(options, loggerFactory);

            var executor = Substitute.For<ObjectResultExecutor>(formatSelector, writerFactory, loggerFactory);
            var result = new ObjectResult(Guid.NewGuid().ToString())
            {
                StatusCode = (int)StatusCode
            };
            var context = Substitute.For<HttpContext>();
            var response = Substitute.For<HttpResponse>();
            var headers = new HeaderDictionary();
            var body = Substitute.For<Stream>();

            response.HasStarted.Returns(false);
            response.HttpContext.Returns(context);
            response.Headers.Returns(headers);
            response.Body.Returns(body);
            context.Response.Returns(response);

            var sut = new ResultExecutor(executor);

            await sut.Execute(context, result).ConfigureAwait(false);

            response.StatusCode.Should().Be((int)StatusCode);
            await executor.Received().ExecuteAsync(
                Arg.Is<ActionContext>(x => x.HttpContext == context),
                Arg.Is<ObjectResult>(x => x == result)).ConfigureAwait(false);
        }

        [Fact]
        public void ExecuteThrowsExceptionWhenResponseStartedTest()
        {
            var mvcOptions = new MvcOptions();
            var options = Substitute.For<IOptions<MvcOptions>>();
            var writerFactory = Substitute.For<IHttpResponseStreamWriterFactory>();
            var loggerFactory = Substitute.For<ILoggerFactory>();

            options.Value.Returns(mvcOptions);
            
            var formatSelector = new DefaultOutputFormatterSelector(options, loggerFactory);

            var executor = new ObjectResultExecutor(formatSelector, writerFactory, loggerFactory);
            var result = new ObjectResult(Guid.NewGuid().ToString());
            var context = Substitute.For<HttpContext>();
            var response = Substitute.For<HttpResponse>();

            response.HasStarted.Returns(true);

            context.Response.Returns(response);

            var sut = new ResultExecutor(executor);

            Func<Task> action = async () => await sut.Execute(context, result).ConfigureAwait(false);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ExecuteThrowsExceptionWithNullContextTest()
        {
            var mvcOptions = new MvcOptions();
            var options = Substitute.For<IOptions<MvcOptions>>();

            options.Value.Returns(mvcOptions);

            var writerFactory = Substitute.For<IHttpResponseStreamWriterFactory>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            
            var formatSelector = new DefaultOutputFormatterSelector(options, loggerFactory);

            var executor = new ObjectResultExecutor(formatSelector, writerFactory, loggerFactory);
            var result = new ObjectResult(Guid.NewGuid().ToString());

            var sut = new ResultExecutor(executor);

            Func<Task> action = async () => await sut.Execute(null, result).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ExecuteThrowsExceptionWithNullResultTest()
        {
            var mvcOptions = new MvcOptions();
            var options = Substitute.For<IOptions<MvcOptions>>();

            options.Value.Returns(mvcOptions);

            var writerFactory = Substitute.For<IHttpResponseStreamWriterFactory>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            
            var formatSelector = new DefaultOutputFormatterSelector(options, loggerFactory);

            var executor = new ObjectResultExecutor(formatSelector, writerFactory, loggerFactory);
            var context = Substitute.For<HttpContext>();

            var sut = new ResultExecutor(executor);

            Func<Task> action = async () => await sut.Execute(context, null).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullExecutorTest()
        {
            Action action = () => new ResultExecutor(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}