namespace TechMentorApi.UnitTests.Security
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Core;
    using TechMentorApi.Security;
    using Xunit;
    using Xunit.Abstractions;

    public class OversizeDataExceptionMiddlewareTests
    {
        private readonly ITestOutputHelper _output;

        public OversizeDataExceptionMiddlewareTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InvokeDoesNotRunExecutorWhenDelegateIsSuccessfulTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();
            var config = Model.Create<AvatarConfig>();

            var sut = new OversizeDataExceptionMiddleware(next, logger, executor, config);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.DidNotReceive().Execute(Arg.Any<HttpContext>(), Arg.Any<ObjectResult>())
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task InvokeSendsBadRequestToExecutorWhenDelegateThrowsInvalidDataExceptionTest()
        {
            var config = Model.Create<AvatarConfig>();

            var expectedMessage = string.Format(CultureInfo.InvariantCulture,
                "Multipart body length limit {0} exceeded", config.MaxLength);

            RequestDelegate next = delegate { throw new InvalidDataException(expectedMessage); };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();

            var sut = new OversizeDataExceptionMiddleware(next, logger, executor, config);

            await sut.Invoke(context).ConfigureAwait(false);

            await executor.Received().Execute(
                    context,
                    Arg.Is<ObjectResult>(x => x.StatusCode == (int) HttpStatusCode.BadRequest))
                .ConfigureAwait(false);
        }

        [Fact]
        public void InvokeThrowsExceptionWhenDelegateThrowsInvalidDataExceptionWithUnexpectedMessageTest()
        {
            RequestDelegate next = delegate { throw new InvalidDataException(); };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();
            var config = Model.Create<AvatarConfig>();
            var response = Substitute.For<HttpResponse>();

            context.Response.Returns(response);
            response.HasStarted.Returns(true);

            var sut = new OversizeDataExceptionMiddleware(next, logger, executor, config);

            Func<Task> action = async () => await sut.Invoke(context).ConfigureAwait(false);

            action.ShouldThrow<InvalidDataException>();
        }

        [Fact]
        public void InvokeThrowsExceptionWhenDelegateThrowsOtherExceptionTest()
        {
            RequestDelegate next = delegate { throw new InvalidOperationException(); };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var context = Substitute.For<HttpContext>();
            var config = Model.Create<AvatarConfig>();
            var response = Substitute.For<HttpResponse>();

            context.Response.Returns(response);
            response.HasStarted.Returns(true);

            var sut = new OversizeDataExceptionMiddleware(next, logger, executor, config);

            Func<Task> action = async () => await sut.Invoke(context).ConfigureAwait(false);

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void InvokeThrowsExceptionWithNullContextTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var config = Model.Create<AvatarConfig>();

            var sut = new OversizeDataExceptionMiddleware(next, logger, executor, config);

            Func<Task> action = async () => await sut.Invoke(null).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();

            Action action = () => new OversizeDataExceptionMiddleware(next, logger, executor, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullDelegateTest()
        {
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var executor = Substitute.For<IResultExecutor>();
            var config = Model.Create<AvatarConfig>();

            Action action = () => new OversizeDataExceptionMiddleware(null, logger, executor, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullExecutorTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var logger = _output.BuildLoggerFor<OversizeDataExceptionMiddleware>();
            var config = Model.Create<AvatarConfig>();

            Action action = () => new OversizeDataExceptionMiddleware(next, logger, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullLoggerTest()
        {
            RequestDelegate next = delegate { return Task.CompletedTask; };
            var executor = Substitute.For<IResultExecutor>();
            var config = Model.Create<AvatarConfig>();

            Action action = () => new OversizeDataExceptionMiddleware(next, null, executor, config);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}