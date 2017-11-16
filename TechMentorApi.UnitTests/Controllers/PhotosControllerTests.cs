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
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using Xunit;

    public class PhotosControllerTests
    {
        [Fact]
        public void CreateThrowsExceptionWithNullCommandTest()
        {
            Action action = () => new PhotosController(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task PostCreatesNewPhotoTest()
        {
            var account = Model.Create<Account>();
            var user = ClaimsIdentityFactory.BuildPrincipal(account);
            var photoDetails = Model.Create<PhotoDetails>();

            var command = Substitute.For<IPhotoCommand>();
            var model = Substitute.For<IFormFile>();
            var httpContext = Substitute.For<HttpContext>();

            httpContext.User = user;

            var routerData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routerData, actionDescriptor);
            var controllerContext = new ControllerContext(actionContext);

            using (var data = new MemoryStream())
            {
                using (var tokenSource = new CancellationTokenSource())
                {
                    model.OpenReadStream().Returns(data);

                    using (var sut = new PhotosController(command))
                    {
                        sut.ControllerContext = controllerContext;

                        command.CreatePhoto(
                                Arg.Is<Photo>(x =>
                                    x.ContentType == model.ContentType && x.ProfileId == account.Id && x.Data == data),
                                tokenSource.Token)
                            .Returns(photoDetails);

                        var actual = await sut.Post(model, tokenSource.Token).ConfigureAwait(false);

                        var result = actual.Should().BeOfType<CreatedAtRouteResult>().Which;

                        result.RouteName.Should().Be("ProfilePhoto");
                        result.RouteValues["profileId"].Should().Be(photoDetails.ProfileId);
                        result.RouteValues["photoId"].Should().Be(photoDetails.Id);

                        var value = result.Value.Should().BeOfType<PhotoDetails>().Which;

                        value.ShouldBeEquivalentTo(photoDetails);
                    }
                }
            }
        }

        [Fact]
        public async Task PostReturnsBadRequestWithNullProfileIdTest()
        {
            var command = Substitute.For<IPhotoCommand>();

            var sut = new PhotosController(command);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.Post(null, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int) HttpStatusCode.BadRequest);
            }
        }
    }
}