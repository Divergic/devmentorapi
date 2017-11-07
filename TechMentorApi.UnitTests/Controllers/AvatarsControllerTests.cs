namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;
    using Xunit;

    public class AvatarsControllerTests
    {
        [Fact]
        public void CreateThrowsExceptionWithNullCommandTest()
        {
            Action action = () => new AvatarsController(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task PostCreatesNewAvatarTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);

            var command = Substitute.For<IAvatarCommand>();
            var model = Substitute.For<IFormFile>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var data = new MemoryStream())
            {
                var avatar = Substitute.For<Avatar>();

                Model.Ignoring<Avatar>(x => x.Data).Populate(avatar);

                using (var tokenSource = new CancellationTokenSource())
                {
                    model.OpenReadStream().Returns(data);

                    using (var sut = new AvatarsController(command))
                    {
                        sut.ControllerContext = controllerContext;

                        command.CreateAvatar(
                                Arg.Is<Avatar>(x =>
                                    x.ContentType == model.ContentType && x.ProfileId == account.Id && x.Data == data),
                                tokenSource.Token)
                            .Returns(avatar);

                        var actual = await sut.Post(model, tokenSource.Token).ConfigureAwait(false);

                        var result = actual.Should().BeOfType<CreatedAtRouteResult>().Which;

                        result.RouteName.Should().Be("ProfileAvatar");
                        result.RouteValues["profileId"].Should().Be(avatar.ProfileId);
                        result.RouteValues["avatarId"].Should().Be(avatar.Id);

                        var value = result.Value.Should().BeOfType<AvatarDetails>().Which;

                        value.ETag.Should().Be(avatar.ETag);
                        value.Id.Should().Be(avatar.Id);
                        value.ProfileId.Should().Be(avatar.ProfileId);

                        avatar.Received().Dispose();
                    }
                }
            }
        }

        [Fact]
        public async Task PostReturnsBadRequestWithNullProfileIdTest()
        {
            var command = Substitute.For<IAvatarCommand>();

            var sut = new AvatarsController(command);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.Post(null, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int) HttpStatusCode.BadRequest);
            }
        }
    }
}