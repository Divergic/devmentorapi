namespace TechMentorApi.Azure
{
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public class PhotoStore : IPhotoStore
    {
        private const string ContainerName = "photos";
        private readonly IStorageConfiguration _configuration;

        public PhotoStore(IStorageConfiguration configuration)
        {
            Ensure.Any.IsNotNull(configuration, nameof(configuration));
            Ensure.String.IsNotNullOrWhiteSpace(configuration.ConnectionString, nameof(configuration.ConnectionString));

            _configuration = configuration;
        }

        public async Task DeletePhoto(Guid profileId, Guid photoId, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));
            Ensure.Guid.IsNotEmpty(photoId, nameof(photoId));

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
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));
            Ensure.Guid.IsNotEmpty(photoId, nameof(photoId));

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

        public async Task<IEnumerable<Guid>> GetPhotos(Guid profileId, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));

            var profilePath = GetProfileBlobFolder(profileId);
            var client = GetClient();
            var container = client.GetContainerReference(ContainerName);
            var directory = container.GetDirectoryReference(profilePath);
            var items = new List<CloudBlockBlob>();

            try
            {
                BlobContinuationToken token = null;

                do
                {
                    var segment = await directory.ListBlobsSegmentedAsync(false, BlobListingDetails.None, null, token, null, null, cancellationToken).ConfigureAwait(false);

                    if (segment == null)
                    {
                        break;
                    }

                    var blobItems = segment.Results.OfType<CloudBlockBlob>();

                    items.AddRange(blobItems);

                    token = segment.ContinuationToken;
                } while (token != null &&
                         cancellationToken.IsCancellationRequested == false);

                // The file names are the photo id values
                return items.Select(x => Guid.Parse(x.Uri.Segments.Last()));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != 404)
                {
                    return new List<Guid>();
                }

                // Check if this 404 is because of the container not existing
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "ContainerNotFound")
                {
                    return new List<Guid>();
                }

                // This is an unknown failure scenario
                throw;
            }
        }

        public async Task<PhotoDetails> StorePhoto(Photo photo, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(photo, nameof(photo));

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
            var profileFolderPath = GetProfileBlobFolder(photo.ProfileId);
            var photoFilename = photo.Id.ToString().ToLowerInvariant();
            var blobReference = profileFolderPath + "/" + photoFilename;

            return blobReference;
        }

        private static string GetProfileBlobFolder(Guid profileId)
        {
            var profileSegment = profileId.ToString().ToLowerInvariant();
            var partition = profileSegment.Substring(0, 1);
            var folderPath = partition + "/" + profileSegment;

            return folderPath;
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