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

    public class PhotoControllerTests
    {
        [Fact]
        public async Task GetReturnsPhotoDataTest()
        {
            var profileId = Guid.NewGuid();
            var photoId = Guid.NewGuid();
            var buffer = Model.Create<byte[]>();

            var query = Substitute.For<IPhotoQuery>();

            using (var data = new MemoryStream(buffer))
            {
                var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.Data = data)
                    .Set(x => x.ContentType = "image/jpg");

                using (var tokenSource = new CancellationTokenSource())
                {
                    query.GetPhoto(profileId, photoId, tokenSource.Token).Returns(photo);

                    using (var target = new PhotoController(query))
                    {
                        var actual = await target.Get(profileId, photoId, tokenSource.Token).ConfigureAwait(false);

                        var result = actual.Should().BeOfType<FileStreamResult>().Which;

                        result.FileStream.Should().BeSameAs(data);
                        result.ContentType.Should().Be(photo.ContentType);
                    }
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWhenPhotoNotFoundTest()
        {
            var profileId = Guid.NewGuid();
            var photoId = Guid.NewGuid();

            var query = Substitute.For<IPhotoQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new PhotoController(query))
                {
                    var actual = await target.Get(profileId, photoId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWithEmptyPhotoIdTest()
        {
            var profileId = Guid.NewGuid();
            var photoId = Guid.Empty;

            var query = Substitute.For<IPhotoQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new PhotoController(query))
                {
                    var actual = await target.Get(profileId, photoId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private async Task GetReturnsNotFoundWithEmptyProfileIdTest()
        {
            var profileId = Guid.NewGuid();
            var photoId = Guid.Empty;

            var query = Substitute.For<IPhotoQuery>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new PhotoController(query))
                {
                    var actual = await target.Get(profileId, photoId, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<ErrorMessageResult>();
                }
            }
        }

        [Fact]
        private void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            Action action = () => new PhotoController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}