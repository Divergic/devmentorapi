namespace TechMentorApi.Azure.IntegrationTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.WindowsAzure.Storage;
    using ModelBuilder;
    using TechMentorApi.Model;
    using Xunit;

    public class PhotoStoreTests
    {
        [Fact]
        public async Task DeletePhotoRemovesPhotoBlobTest()
        {
            var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new PhotoStore(Config.Storage);

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);
            }

            await sut.DeletePhoto(photo.ProfileId, photo.Id, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetPhoto(photo.ProfileId, photo.Id, CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task DeletePhotoReturnsSuccessfullyWhenBlobNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("photos");

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var sut = new PhotoStore(Config.Storage);

            // This should not throw an exception
            await sut.DeletePhoto(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeletePhotoReturnsSuccessfullyWhenContainerNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("photos");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var sut = new PhotoStore(Config.Storage);

            // This should not throw an exception
            await sut.DeletePhoto(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetPhotoReturnsNullWhenBlobNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("photos");

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var sut = new PhotoStore(Config.Storage);

            var actual = await sut.GetPhoto(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetPhotoReturnsNullWhenContainerNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("photos");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var sut = new PhotoStore(Config.Storage);

            var actual = await sut.GetPhoto(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetPhotoReturnsStoredPhotoTest()
        {
            var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new PhotoStore(Config.Storage);

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                var photoDetails = await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);

                photoDetails.Should().BeEquivalentTo(photo, opt => opt.ExcludingMissingMembers().Excluding(x => x.Hash));
                photoDetails.Hash.Should().NotBeNullOrWhiteSpace();

                using (var retrievedPhoto = await sut.GetPhoto(photo.ProfileId, photo.Id, CancellationToken.None)
                    .ConfigureAwait(false))
                {
                    retrievedPhoto.Should().BeEquivalentTo(photoDetails, opt => opt.ExcludingMissingMembers());

                    using (var outputStream = retrievedPhoto.Data)
                    {
                        outputStream.Position.Should().Be(0);
                        outputStream.Length.Should().Be(expected.Length);

                        var actual = new byte[expected.Length];

                        var length = outputStream.Read(actual, 0, expected.Length);

                        length.Should().Be(expected.Length);
                        expected.SequenceEqual(actual).Should().BeTrue();
                    }
                }
            }
        }

        [Fact]
        public void GetPhotoThrowsExceptionWithEmptyPhotoIdTest()
        {
            var sut = new PhotoStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetPhoto(Guid.NewGuid(), Guid.Empty, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetPhotoThrowsExceptionWithEmptyProfileIdTest()
        {
            var sut = new PhotoStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetPhoto(Guid.Empty, Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task StorePhotoCreatesContainerWhenNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("photos");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new PhotoStore(Config.Storage);

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                var photoDetails = await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);

                photoDetails.Should().BeEquivalentTo(photo, opt => opt.ExcludingMissingMembers().Excluding(x => x.Hash));
                photoDetails.Hash.Should().NotBeNullOrWhiteSpace();

                var retrievedPhoto = await sut.GetPhoto(photo.ProfileId, photo.Id, CancellationToken.None)
                    .ConfigureAwait(false);

                retrievedPhoto.Should().BeEquivalentTo(photoDetails, opt => opt.ExcludingMissingMembers());

                using (var outputStream = retrievedPhoto.Data)
                {
                    outputStream.Position.Should().Be(0);
                    outputStream.Length.Should().Be(expected.Length);

                    var actual = new byte[expected.Length];

                    var length = outputStream.Read(actual, 0, expected.Length);

                    length.Should().Be(expected.Length);
                    expected.SequenceEqual(actual).Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task StorePhotoStoresNewPhotoTest()
        {
            var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new PhotoStore(Config.Storage);

            using (var inputStream = new MemoryStream(expected))
            {
                photo.Data = inputStream;

                var photoDetails = await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);

                photoDetails.Should().BeEquivalentTo(photo, opt => opt.ExcludingMissingMembers().Excluding(x => x.Hash));
                photoDetails.Hash.Should().NotBeNullOrWhiteSpace();
                var retrievedPhoto = await sut.GetPhoto(photo.ProfileId, photo.Id, CancellationToken.None)
                    .ConfigureAwait(false);

                retrievedPhoto.Should().BeEquivalentTo(photoDetails, opt => opt.ExcludingMissingMembers());

                using (var outputStream = retrievedPhoto.Data)
                {
                    outputStream.Position.Should().Be(0);
                    outputStream.Length.Should().Be(expected.Length);

                    var actual = new byte[expected.Length];

                    var length = outputStream.Read(actual, 0, expected.Length);

                    length.Should().Be(expected.Length);
                    expected.SequenceEqual(actual).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void StorePhotoThrowsExceptionWithNullPhotoTest()
        {
            var sut = new PhotoStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.StorePhoto(null, CancellationToken.None).ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task StorePhotoUpdatesExistingPhotoTest()
        {
            var photo = Model.Ignoring<Photo>(x => x.Data).Create<Photo>().Set(x => x.ContentType = ".jpg");
            var original = Model.Create<byte[]>();

            var sut = new PhotoStore(Config.Storage);

            using (var firstInputStream = new MemoryStream(original))
            {
                photo.Data = firstInputStream;

                var firstStoredPhoto = await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);

                firstStoredPhoto.Should().BeEquivalentTo(photo, opt => opt.Excluding(x => x.Hash));
                firstStoredPhoto.Hash.Should().NotBeNullOrWhiteSpace();
            }

            var expected = Model.Create<byte[]>();

            using (var secondInputStream = new MemoryStream(expected))
            {
                photo.Data = secondInputStream;
                photo.ContentType = ".png";

                var photoDetails = await sut.StorePhoto(photo, CancellationToken.None).ConfigureAwait(false);

                photoDetails.Should().BeEquivalentTo(photo, opt => opt.ExcludingMissingMembers().Excluding(x => x.Hash));
                photoDetails.Hash.Should().NotBeNullOrWhiteSpace();

                var retrievedPhoto = await sut.GetPhoto(photo.ProfileId, photo.Id, CancellationToken.None)
                    .ConfigureAwait(false);

                retrievedPhoto.Should().BeEquivalentTo(photoDetails, opt => opt.ExcludingMissingMembers());

                using (var outputStream = retrievedPhoto.Data)
                {
                    outputStream.Position.Should().Be(0);
                    outputStream.Length.Should().Be(expected.Length);

                    var actual = new byte[expected.Length];

                    var length = outputStream.Read(actual, 0, expected.Length);

                    length.Should().Be(expected.Length);
                    expected.SequenceEqual(actual).Should().BeTrue();
                    original.SequenceEqual(actual).Should().BeFalse();
                }
            }
        }
    }
}