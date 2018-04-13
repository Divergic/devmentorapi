namespace TechMentorApi.Business.UnitTests.Queries
{
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using NSubstitute.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;
    using Xunit;

    public class PhotoQueryTests
    {
        [Fact]
        public async Task GetPhotoReturnsPhotoFromStoreTest()
        {
            var expected = Model.Ignoring<Photo>(x => x.Data).Create<Photo>();

            var store = Substitute.For<IPhotoStore>();

            var sut = new PhotoQuery(store);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetPhoto(expected.ProfileId, expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPhoto(expected.ProfileId, expected.Id, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetPhotosReturnsPhotosFromStoreTestAsync()
        {
            var profileId = Guid.NewGuid();
            var photoReferences = Model.Create<List<Guid>>();
            var photos = Model.Ignoring<Photo>(x => x.Data).Create<List<Photo>>();

            var store = Substitute.For<IPhotoStore>();

            var sut = new PhotoQuery(store);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetPhotos(profileId, tokenSource.Token).Returns(photoReferences);
                store.GetPhoto(profileId, Arg.Any<Guid>(), tokenSource.Token).Returns((CallInfo info) =>
                {
                    var photoId = info.ArgAt<Guid>(1);
                    var index = photoReferences.IndexOf(photoId);

                    return photos[index];
                });

                var actual = await sut.GetPhotos(profileId, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEquivalentTo(photos);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullStoreTest()
        {
            Action action = () => new PhotoQuery(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}