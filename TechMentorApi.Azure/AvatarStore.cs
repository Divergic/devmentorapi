namespace TechMentorApi.Azure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using TechMentorApi.Model;

    public class AvatarStore : IAvatarStore
    {
        private readonly IStorageConfiguration _configuration;

        public AvatarStore(IStorageConfiguration configuration)
        {
            Ensure.That(configuration, nameof(configuration)).IsNotNull();
            Ensure.That(configuration.ConnectionString, nameof(configuration.ConnectionString)).IsNotNullOrWhiteSpace();

            _configuration = configuration;
        }

        public async Task DeleteAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken)
        {
            Ensure.That(profileId, nameof(profileId)).IsNotEmpty();
            Ensure.That(avatarId, nameof(avatarId)).IsNotEmpty();

            var avatar = new Avatar
            {
                Id = avatarId,
                ProfileId = profileId
            };

            var blobReference = GetBlobReference(avatar);

            var client = GetClient();

            var container = client.GetContainerReference("avatars");

            var blockBlob = container.GetBlockBlobReference(blobReference);

            try
            {
                await blockBlob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                // Check if this 404 is because of the container not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "ContainerNotFound")
                {
                    return;
                }

                // Check if this 404 is because of the blob not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "BlobNotFound")
                {
                    return;
                }

                // This is an unknown failure scenario
                throw;
            }
        }

        public async Task<Avatar> GetAvatar(Guid profileId, Guid avatarId, CancellationToken cancellationToken)
        {
            Ensure.That(profileId, nameof(profileId)).IsNotEmpty();
            Ensure.That(avatarId, nameof(avatarId)).IsNotEmpty();

            var avatar = new Avatar
            {
                Id = avatarId,
                ProfileId = profileId
            };

            var blobReference = GetBlobReference(avatar);

            var client = GetClient();

            var container = client.GetContainerReference("avatars");

            var blockBlob = container.GetBlockBlobReference(blobReference);

            try
            {
                var stream = new MemoryStream();

                await blockBlob.DownloadToStreamAsync(stream, null, null, null, cancellationToken)
                    .ConfigureAwait(false);

                stream.Position = 0;

                avatar.Data = stream;
                avatar.Extension = blockBlob.Metadata[nameof(Avatar.Extension)];
                avatar.SetETag(blockBlob.Properties.ETag);

                return avatar;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                // Check if this 404 is because of the container not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "ContainerNotFound")
                {
                    return null;
                }

                // Check if this 404 is because of the blob not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "BlobNotFound")
                {
                    return null;
                }

                // This is an unknown failure scenario
                throw;
            }
        }

        public async Task<Avatar> StoreAvatar(Avatar avatar,
            CancellationToken cancellationToken)
        {
            Ensure.That(avatar, nameof(avatar)).IsNotNull();

            var blobReference = GetBlobReference(avatar);

            var client = GetClient();

            var container = client.GetContainerReference("avatars");

            var blockBlob = container.GetBlockBlobReference(blobReference);

            try
            {
                blockBlob.Metadata[nameof(Avatar.Extension)] = avatar.Extension;

                avatar.Data.Position = 0;

                await blockBlob.UploadFromStreamAsync(avatar.Data, null, null, null, cancellationToken)
                    .ConfigureAwait(false);
                await blockBlob.SetPropertiesAsync(null, null, null, cancellationToken).ConfigureAwait(false);

                avatar.SetETag(blockBlob.Properties.ETag);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                // Check if this 404 is because of the container not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "ContainerNotFound")
                {
                    await container.CreateAsync(BlobContainerPublicAccessType.Container, null, null, cancellationToken)
                        .ConfigureAwait(false);

                    avatar.Data.Position = 0;

                    await blockBlob.UploadFromStreamAsync(avatar.Data, null, null, null, cancellationToken)
                        .ConfigureAwait(false);
                    await blockBlob.SetPropertiesAsync(null, null, null, cancellationToken).ConfigureAwait(false);

                    avatar.SetETag(blockBlob.Properties.ETag);
                }
                else
                {
                    // This is an unknown failure scenario
                    throw;
                }
            }

            return avatar;
        }

        private static string GetBlobReference(Avatar avatar)
        {
            var profileSegment = avatar.ProfileId.ToString().ToLowerInvariant();
            var partition = profileSegment.Substring(0, 1).ToLowerInvariant();
            var avatarFilename = avatar.Id.ToString().ToLowerInvariant();
            var blobReference = partition + "\\" + profileSegment + "\\" + avatarFilename;

            return blobReference;
        }

        private CloudBlobClient GetClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration.ConnectionString);

            return storageAccount.CreateCloudBlobClient();
        }
    }
}