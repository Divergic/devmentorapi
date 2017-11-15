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

    public class PhotoStore : IPhotoStore
    {
        private const string ContainerName = "photos";
        private readonly IStorageConfiguration _configuration;

        public PhotoStore(IStorageConfiguration configuration)
        {
            Ensure.That(configuration, nameof(configuration)).IsNotNull();
            Ensure.That(configuration.ConnectionString, nameof(configuration.ConnectionString)).IsNotNullOrWhiteSpace();

            _configuration = configuration;
        }

        public async Task DeletePhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken)
        {
            Ensure.That(profileId, nameof(profileId)).IsNotEmpty();
            Ensure.That(photoId, nameof(photoId)).IsNotEmpty();

            var photo = new Photo
            {
                Id = photoId,
                ProfileId = profileId
            };

            var blobReference = GetBlobReference(photo);

            var client = GetClient();

            var container = client.GetContainerReference(ContainerName);

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

        public async Task<Photo> GetPhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken)
        {
            Ensure.That(profileId, nameof(profileId)).IsNotEmpty();
            Ensure.That(photoId, nameof(photoId)).IsNotEmpty();

            var photo = new Photo
            {
                Id = photoId,
                ProfileId = profileId
            };

            var blobReference = GetBlobReference(photo);

            var client = GetClient();

            var container = client.GetContainerReference(ContainerName);

            var blockBlob = container.GetBlockBlobReference(blobReference);

            try
            {
                var stream = new MemoryStream();

                await blockBlob.DownloadToStreamAsync(stream, null, null, null, cancellationToken)
                    .ConfigureAwait(false);

                stream.Position = 0;

                photo.Data = stream;
                photo.ContentType = blockBlob.Metadata[nameof(Photo.ContentType)];
                photo.SetETag(blockBlob.Properties.ETag);

                return photo;
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

        public async Task<PhotoDetails> StorePhoto(Photo photo,
            CancellationToken cancellationToken)
        {
            Ensure.That(photo, nameof(photo)).IsNotNull();

            var blobReference = GetBlobReference(photo);

            var client = GetClient();

            var container = client.GetContainerReference(ContainerName);

            var blockBlob = container.GetBlockBlobReference(blobReference);

            try
            {
                blockBlob.Metadata[nameof(Photo.ContentType)] = photo.ContentType;

                photo.Data.Position = 0;

                await blockBlob.UploadFromStreamAsync(photo.Data, null, null, null, cancellationToken)
                    .ConfigureAwait(false);
                await blockBlob.SetPropertiesAsync(null, null, null, cancellationToken).ConfigureAwait(false);

                photo.SetETag(blockBlob.Properties.ETag);
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

                    photo.Data.Position = 0;

                    await blockBlob.UploadFromStreamAsync(photo.Data, null, null, null, cancellationToken)
                        .ConfigureAwait(false);
                    await blockBlob.SetPropertiesAsync(null, null, null, cancellationToken).ConfigureAwait(false);

                    photo.SetETag(blockBlob.Properties.ETag);
                }
                else
                {
                    // This is an unknown failure scenario
                    throw;
                }
            }

            var details = new PhotoDetails
            {
                Id = photo.Id,
                ProfileId = photo.ProfileId,
                ETag = photo.ETag
            };
            
            return details;
        }

        private static string GetBlobReference(Photo photo)
        {
            var profileSegment = photo.ProfileId.ToString().ToLowerInvariant();
            var partition = profileSegment.Substring(0, 1).ToLowerInvariant();
            var photoFilename = photo.Id.ToString().ToLowerInvariant();
            var blobReference = partition + "\\" + profileSegment + "\\" + photoFilename;

            return blobReference;
        }

        private CloudBlobClient GetClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration.ConnectionString);

            return storageAccount.CreateCloudBlobClient();
        }
    }
}