using FluentAssertions;
using ModelBuilder;
using NSubstitute;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TechMentorApi.Management.UnitTests
{
    public class UserStoreTests
    {
        [Fact]
        public async Task DeleteUserRemovesUserFromServiceTest()
        {
            var username = Guid.NewGuid().ToString();
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = true);

            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"SomeToken\",\"scope\":\"delete: users\",\"expires_in\":86400,\"token_type\":\"Bearer\"}")
            };
            var successResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            var sut = new UserStore(config, client);

            using (var tokenSource = new CancellationTokenSource())
            {
                handler.WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).Returns(tokenResponse, successResponse);

                await sut.DeleteUser(username, tokenSource.Token).ConfigureAwait(false);

                await handler.Received(2).WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).ConfigureAwait(false);
                await handler.Received(2).WrapperSendAsync(Arg.Is<HttpRequestMessage>(x => x.RequestUri.ToString().Contains(config.Tenant)), tokenSource.Token).ConfigureAwait(false);
                await handler.Received(1).WrapperSendAsync(Arg.Is<HttpRequestMessage>(x => x.RequestUri.ToString().Contains(username)), tokenSource.Token).ConfigureAwait(false);
                await handler.Received(1).WrapperSendAsync(Arg.Is<HttpRequestMessage>(x => x.Headers.Authorization.ToString() == "Bearer SomeToken"), tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteUserSkipsProcessingWhenIsEnabledIsFalseTest()
        {
            var username = Guid.NewGuid().ToString();
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = false);

            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"SomeToken\",\"scope\":\"delete: users\",\"expires_in\":86400,\"token_type\":\"Bearer\"}")
            };
            var successResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            var sut = new UserStore(config, client);

            using (var tokenSource = new CancellationTokenSource())
            {
                handler.WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).Returns(tokenResponse, successResponse);

                await sut.DeleteUser(username, tokenSource.Token).ConfigureAwait(false);

                await handler.DidNotReceive().WrapperSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteUserThrowsExceptionWhenFailingToDeleteUserTest()
        {
            var username = Guid.NewGuid().ToString();
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = true);

            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"SomeToken\",\"scope\":\"delete: users\",\"expires_in\":86400,\"token_type\":\"Bearer\"}")
            };
            var failureResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var sut = new UserStore(config, client);

            using (var tokenSource = new CancellationTokenSource())
            {
                handler.WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).Returns(tokenResponse, failureResponse);

                Func<Task> action = async () => await sut.DeleteUser(username, tokenSource.Token).ConfigureAwait(false);

                action.Should().Throw<InvalidOperationException>();

                await handler.Received(2).WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteUserThrowsExceptionWhenFailingToGetAccessTokenTest()
        {
            var username = Guid.NewGuid().ToString();
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = true);
            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            var failureResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var sut = new UserStore(config, client);

            using (var tokenSource = new CancellationTokenSource())
            {
                handler.WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).Returns(failureResponse);

                Func<Task> action = async () => await sut.DeleteUser(username, tokenSource.Token).ConfigureAwait(false);

                action.Should().Throw<InvalidOperationException>();

                await handler.Received(1).WrapperSendAsync(Arg.Any<HttpRequestMessage>(), tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void DeleteUserThrowsExceptionWithInvalidUsernameTest(string username)
        {
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = true);
            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            var sut = new UserStore(config, client);

            Func<Task> action = async () => await sut.DeleteUser(username, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullClientTest()
        {
            var config = Model.Create<Auth0ManagementConfig>().Set(x => x.IsEnabled = true);

            Action action = () => new UserStore(config, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigTest()
        {
            var handler = Substitute.For<WrapperHandler>();
            var client = new WrapperInvoker(handler);

            Action action = () => new UserStore(null, client);

            action.Should().Throw<ArgumentNullException>();
        }

        public abstract class WrapperHandler : HttpMessageHandler
        {
            public abstract Task<HttpResponseMessage> WrapperSendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return WrapperSendAsync(request, cancellationToken);
            }
        }

        public class WrapperInvoker : HttpMessageInvoker
        {
            private readonly WrapperHandler _handler;

            public WrapperInvoker(WrapperHandler handler) : base(handler)
            {
                _handler = handler;
            }

            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handler.WrapperSendAsync(request, cancellationToken);
            }
        }
    }
}