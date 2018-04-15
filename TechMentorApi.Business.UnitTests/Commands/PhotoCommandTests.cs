namespace TechMentorApi.Business.UnitTests.Commands
{
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;

    public class PhotoCommandTests
    {
        [Fact]
        public async Task CreatePhotoStoresResizedPhotoTest()
        {
            var expected = Model.Ignoring<Photo>(x => x.Data).Create<Photo>();
            var details = Model.Create<PhotoDetails>();

            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();
            var resizedPhoto = Substitute.For<Photo>();

            config.MaxHeight.Returns(Environment.TickCount);
            config.MaxWidth = config.MaxHeight + 1;

            var sut = new PhotoCommand(store, resizer, config);

            using (resizedPhoto)
            {
                resizer.Resize(expected, config.MaxHeight, config.MaxWidth).Returns(resizedPhoto);

                using (var tokenSource = new CancellationTokenSource())
                {
                    store.StorePhoto(resizedPhoto, tokenSource.Token).Returns(details);

                    var actual = await sut.CreatePhoto(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeEquivalentTo(details);

                    resizedPhoto.Received().Dispose();
                }
            }
        }

        [Fact]
        public void CreatePhotoThrowsExceptionWithNullPhotoTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();

            var sut = new PhotoCommand(store, resizer, config);

            Func<Task> action = async () => await sut.CreatePhoto(null, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task DeletePhotosRemovesPhotosFromStoreTest()
        {
            var profileId = Guid.NewGuid();
            var photoReferences = Model.Create<List<Guid>>();

            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetPhotos(profileId, tokenSource.Token).Returns(photoReferences);

                var sut = new PhotoCommand(store, resizer, config);

                await sut.DeletePhotos(profileId, tokenSource.Token).ConfigureAwait(false);

                await store.Received(photoReferences.Count).DeletePhoto(profileId, Arg.Any<Guid>(), tokenSource.Token).ConfigureAwait(false);

                foreach (var photoReference in photoReferences)
                {
                    await store.Received().DeletePhoto(profileId, photoReference, tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public void DeletePhotosThrowsExceptionWithEmptyProfileIdTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();

            var sut = new PhotoCommand(store, resizer, config);

            Func<Task> action = async () => await sut.DeletePhotos(Guid.Empty, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();

            Action action = () => new PhotoCommand(store, resizer, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullResizerTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var config = Substitute.For<IPhotoConfig>();

            Action action = () => new PhotoCommand(store, null, config);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullStoreTest()
        {
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();

            Action action = () => new PhotoCommand(null, resizer, config);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}