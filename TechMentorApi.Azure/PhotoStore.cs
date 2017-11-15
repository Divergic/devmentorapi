namespace TechMentorApi.Azure
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
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
                photo.Hash = blockBlob.Metadata[nameof(Photo.Hash)];

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

            photo.Data.Position = 0;

            var buffer = new byte[photo.Data.Length];

            await photo.Data.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

            var hash = GetBlobHash(buffer);

            blockBlob.Metadata[nameof(Photo.ContentType)] = photo.ContentType;
            blockBlob.Metadata[nameof(Photo.Hash)] = hash;
            photo.Hash = hash;

            try
            {
                await blockBlob.UploadFromByteArrayAsync(buffer, 0, buffer.Length, null, null, null, cancellationToken)
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
                    await container.CreateAsync(BlobContainerPublicAccessType.Container, null, null, cancellationToken)
                        .ConfigureAwait(false);

                    await blockBlob
                        .UploadFromByteArrayAsync(buffer, 0, buffer.Length, null, null, null, cancellationToken)
                        .ConfigureAwait(false);
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
                Hash = photo.Hash
            };

            return details;
        }

        private static string GetBlobHash(byte[] data)
        {
            using (var hashAlgorithm = SHA1.Create())
            {
                var hashBytes = hashAlgorithm.ComputeHash(data);

                return HexStringFromBytes(hashBytes);
            }
        }

        private static string GetBlobReference(Photo photo)
        {
            var profileSegment = photo.ProfileId.ToString().ToLowerInvariant();
            var partition = profileSegment.Substring(0, 1).ToLowerInvariant();
            var photoFilename = photo.Id.ToString().ToLowerInvariant();
            var blobReference = partition + "\\" + profileSegment + "\\" + photoFilename;

            return blobReference;
        }

        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }

            return sb.ToString();
        }

        private CloudBlobClient GetClient()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration.ConnectionString);

            return storageAccount.CreateCloudBlobClient();
        }
    }
}