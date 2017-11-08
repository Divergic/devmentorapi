namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using Xunit;

    public class AvatarControllerTests
    {
        [Fact]
        public async Task GetReturnsAvatarDataTest()
        {
            var profileId = Guid.NewGuid();
            var avatarId = Guid.NewGuid();
            var buffer = Model.Create<byte[]>();

            var query = Substitute.For<IAvatarQuery>();

            using (var data = new MemoryStream(buffer))
            {
                var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.Data = data)
                    .Set(x => x.ContentType = "image/jpg");

                using (var tokenSource = new CancellationTokenSource())
                {
                    query.GetAvatar(profileId, avatarId, tokenSource.Token).Returns(avatar);

                    using (var target = new AvatarController(query))
                    {
                        var actual = await target.Get(profileId, avatarId, tokenSource.Token).ConfigureAwait(false);

                        var result = actual.Should().BeOfType<FileStreamResult>().Which;

                        result.FileStream.Should().BeSameAs(data);
                        result.ContentType.Should().Be(avatar.ContentType);
                    }
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWhenAvatarNotFoundTest()
        {
            var profileId = Guid.NewGuid();
            var avatarId = Guid.NewGuid();

            var query = Substitute.For<IAvatarQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AvatarController(query))
                {
                    var actual = await target.Get(profileId, avatarId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWithEmptyAvatarIdTest()
        {
            var profileId = Guid.NewGuid();
            var avatarId = Guid.Empty;

            var query = Substitute.For<IAvatarQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AvatarController(query))
                {
                    var actual = await target.Get(profileId, avatarId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWithEmptyProfileIdTest()
        {
            var profileId = Guid.NewGuid();
            var avatarId = Guid.Empty;

            var query = Substitute.For<IAvatarQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new AvatarController(query))
                {
                    var actual = await target.Get(profileId, avatarId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            Action action = () => new AvatarController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}