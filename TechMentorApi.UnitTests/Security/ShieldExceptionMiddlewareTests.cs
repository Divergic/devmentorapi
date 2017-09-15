namespace TechMentorApi.UnitTests.Security
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using NSubstitute;
    using TechMentorApi.Core;
    using TechMentorApi.Security;
    using Xunit;
    using Xunit.Abstractions;

    public class ShieldExceptionMiddlewareTests
    {
        private readonly ITestOutputHelper _output;

        public ShieldExceptionMiddlewareTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InvokeDoesNotRunExecutorWhenDelegateIsSuccessfulTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();

            var sut = new ShieldExceptionMiddleware(next, logger, executor);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.DidNotReceive().Execute(Arg.Any<HttpContext>(), Arg.Any<ObjectResult>())
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task InvokeDoesNotRunExecutorWhenDelegateThrowsExceptionAndHasStartedSendingResponseTest()
        {
            RequestDelegate next = delegate { throw new InvalidOperationException(); };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();
            var response = Substitute.For<HttpResponse>();

            context.Response.Returns(response);
            response.HasStarted.Returns(true);

            var sut = new ShieldExceptionMiddleware(next, logger, executor);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.DidNotReceive().Execute(Arg.Any<HttpContext>(), Arg.Any<ObjectResult>())
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task InvokeSendsInternalServerErrorToExecutorWhenDelegateThrowsExceptionTest()
        {
            RequestDelegate next = delegate { throw new InvalidOperationException(); };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();

            var sut = new ShieldExceptionMiddleware(next, logger, executor);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.Received().Execute(
                    context,
                    Arg.Is<ObjectResult>(x => x.StatusCode == (int) HttpStatusCode.InternalServerError))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task InvokeSendsNotFoundToExecutorWhenDelegateThrowsNotFoundExceptionTest()
        {
            RequestDelegate next = delegate { throw new NotFoundException(); };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();

            var sut = new ShieldExceptionMiddleware(next, logger, executor);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.Received().Execute(
                    context,
                    Arg.Is<ObjectResult>(x => x.StatusCode == (int) HttpStatusCode.NotFound))
                .ConfigureAwait(false);
        }

        [Fact]
        public void InvokeThrowsExceptionWithNullContextTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();

            var sut = new ShieldExceptionMiddleware(next, logger, executor);

            Func<Task> action = async () => await sut.Invoke(null).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullDelegateTest()
        {
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();

            Action action = () => new ShieldExceptionMiddleware(null, logger, executor);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullExecutorTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<ShieldExceptionMiddleware>();

            Action action = () => new ShieldExceptionMiddleware(next, logger, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullLoggerTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var executor = Substitute.For<IResultExecutor>();

            Action action = () => new ShieldExceptionMiddleware(next, null, executor);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}