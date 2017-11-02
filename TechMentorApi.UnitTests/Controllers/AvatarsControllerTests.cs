using System;
using System.Collections.Generic;
using System.Text;

namespace TechMentorApi.UnitTests.Controllers
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using NSubstitute;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
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
        public async Task PostReturnsNotFoundWithNullProfileIdTest()
        {
            var command = Substitute.For<IAvatarCommand>();
            var model = Substitute.For<IFormFile>();

            var sut = new AvatarsController(command);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.Post(Guid.Empty, model, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>();
            }
        }

        [Fact]
        public async Task PostCreatesNewAvatarTest()
        {
            var profileId = Guid.NewGuid();

            var command = Substitute.For<IAvatarCommand>();
            var model = Substitute.For<IFormFile>();
            var data = Substitute.For<Stream>();

            model.OpenReadStream().Returns(data);

            var sut = new AvatarsController(command);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.Post(profileId, model, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>();
            }
        }
    }
}
