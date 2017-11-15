namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
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

                    actual.ShouldBeEquivalentTo(details);

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

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var resizer = Substitute.For<IPhotoResizer>();

            Action action = () => new PhotoCommand(store, resizer, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullResizerTest()
        {
            var store = Substitute.For<IPhotoStore>();
            var config = Substitute.For<IPhotoConfig>();

            Action action = () => new PhotoCommand(store, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullStoreTest()
        {
            var resizer = Substitute.For<IPhotoResizer>();
            var config = Substitute.For<IPhotoConfig>();

            Action action = () => new PhotoCommand(null, resizer, config);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}