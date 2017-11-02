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

    public class AvatarStoreTests
    {
        [Fact]
        public async Task DeleteAvatarRemovesAvatarBlobTest()
        {
            var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new AvatarStore(Config.Storage);

            using (var inputStream = new MemoryStream(expected))
            {
                avatar.Data = inputStream;

                await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);
            }

            await sut.DeleteAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None).ConfigureAwait(false);

            var actual = await sut.GetAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAvatarReturnsSuccessfullyWhenBlobNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("avatars");

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var sut = new AvatarStore(Config.Storage);

            // This should not throw an exception
            await sut.DeleteAvatar(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteAvatarReturnsSuccessfullyWhenContainerNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("avatars");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var sut = new AvatarStore(Config.Storage);

            // This should not throw an exception
            await sut.DeleteAvatar(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetAvatarReturnsNullWhenBlobNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("avatars");

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            var sut = new AvatarStore(Config.Storage);

            var actual = await sut.GetAvatar(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetAvatarReturnsNullWhenContainerNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("avatars");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var sut = new AvatarStore(Config.Storage);

            var actual = await sut.GetAvatar(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
                .ConfigureAwait(false);

            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetAvatarReturnsStoredAvatarTest()
        {
            var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new AvatarStore(Config.Storage);

            Avatar storedAvatar;

            using (var inputStream = new MemoryStream(expected))
            {
                avatar.Data = inputStream;

                storedAvatar = await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);

                storedAvatar.ShouldBeEquivalentTo(avatar, opt => opt.Excluding(x => x.ETag));
                storedAvatar.ETag.Should().NotBeNullOrWhiteSpace();
            }

            var retrievedAvatar = await sut.GetAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None)
                .ConfigureAwait(false);

            retrievedAvatar.ShouldBeEquivalentTo(storedAvatar, opt => opt.Excluding(x => x.Data));

            using (var outputStream = retrievedAvatar.Data)
            {
                outputStream.Position.Should().Be(0);
                outputStream.Length.Should().Be(expected.Length);

                var actual = new byte[expected.Length];

                var length = outputStream.Read(actual, 0, expected.Length);

                length.Should().Be(expected.Length);
                expected.SequenceEqual(actual).Should().BeTrue();
            }
        }

        [Fact]
        public void GetAvatarThrowsExceptionWithEmptyAvatarIdTest()
        {
            var sut = new AvatarStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetAvatar(Guid.NewGuid(), Guid.Empty, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GetAvatarThrowsExceptionWithEmptyProfileIdTest()
        {
            var sut = new AvatarStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.GetAvatar(Guid.Empty, Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StoreAvatarCreatesContainerWhenNotFoundTest()
        {
            // Retrieve storage account from connection-string
            var storageAccount = CloudStorageAccount.Parse(Config.Storage.ConnectionString);

            // Create the client
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("avatars");

            await container.DeleteIfExistsAsync().ConfigureAwait(false);

            var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new AvatarStore(Config.Storage);

            Avatar storedAvatar;

            using (var inputStream = new MemoryStream(expected))
            {
                avatar.Data = inputStream;

                storedAvatar = await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);

                storedAvatar.ShouldBeEquivalentTo(avatar, opt => opt.Excluding(x => x.ETag));
                storedAvatar.ETag.Should().NotBeNullOrWhiteSpace();
            }

            var retrievedAvatar = await sut.GetAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None)
                .ConfigureAwait(false);

            retrievedAvatar.ShouldBeEquivalentTo(storedAvatar, opt => opt.Excluding(x => x.Data));

            using (var outputStream = retrievedAvatar.Data)
            {
                outputStream.Position.Should().Be(0);
                outputStream.Length.Should().Be(expected.Length);

                var actual = new byte[expected.Length];

                var length = outputStream.Read(actual, 0, expected.Length);

                length.Should().Be(expected.Length);
                expected.SequenceEqual(actual).Should().BeTrue();
            }
        }

        [Fact]
        public async Task StoreAvatarStoresNewAvatarTest()
        {
            var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.ContentType = ".jpg");
            var expected = Model.Create<byte[]>();

            var sut = new AvatarStore(Config.Storage);

            Avatar storedAvatar;

            using (var inputStream = new MemoryStream(expected))
            {
                avatar.Data = inputStream;

                storedAvatar = await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);

                storedAvatar.ShouldBeEquivalentTo(avatar, opt => opt.Excluding(x => x.ETag));
                storedAvatar.ETag.Should().NotBeNullOrWhiteSpace();
            }

            var retrievedAvatar = await sut.GetAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None)
                .ConfigureAwait(false);

            retrievedAvatar.ShouldBeEquivalentTo(storedAvatar, opt => opt.Excluding(x => x.Data));

            using (var outputStream = retrievedAvatar.Data)
            {
                outputStream.Position.Should().Be(0);
                outputStream.Length.Should().Be(expected.Length);

                var actual = new byte[expected.Length];

                var length = outputStream.Read(actual, 0, expected.Length);

                length.Should().Be(expected.Length);
                expected.SequenceEqual(actual).Should().BeTrue();
            }
        }

        [Fact]
        public void StoreAvatarThrowsExceptionWithNullAvatarTest()
        {
            var sut = new AvatarStore(Config.Storage);

            Func<Task> action = async () =>
                await sut.StoreAvatar(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task StoreAvatarUpdatesExistingAvatarTest()
        {
            var avatar = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>().Set(x => x.ContentType = ".jpg");
            var original = Model.Create<byte[]>();

            var sut = new AvatarStore(Config.Storage);

            using (var firstInputStream = new MemoryStream(original))
            {
                avatar.Data = firstInputStream;

                var firstStoredAvatar = await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);

                firstStoredAvatar.ShouldBeEquivalentTo(avatar, opt => opt.Excluding(x => x.ETag));
                firstStoredAvatar.ETag.Should().NotBeNullOrWhiteSpace();
            }

            var expected = Model.Create<byte[]>();

            Avatar secondStoredAvatar;

            using (var secondInputStream = new MemoryStream(expected))
            {
                avatar.Data = secondInputStream;
                avatar.ContentType = ".png";

                secondStoredAvatar = await sut.StoreAvatar(avatar, CancellationToken.None).ConfigureAwait(false);

                secondStoredAvatar.ShouldBeEquivalentTo(avatar, opt => opt.Excluding(x => x.ETag));
                secondStoredAvatar.ETag.Should().NotBeNullOrWhiteSpace();
            }

            var retrievedAvatar = await sut.GetAvatar(avatar.ProfileId, avatar.Id, CancellationToken.None)
                .ConfigureAwait(false);

            retrievedAvatar.ShouldBeEquivalentTo(secondStoredAvatar, opt => opt.Excluding(x => x.Data));

            using (var outputStream = retrievedAvatar.Data)
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