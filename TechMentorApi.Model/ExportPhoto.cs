using EnsureThat;
using System;

namespace TechMentorApi.Model
{
    public class ExportPhoto
    {
        public ExportPhoto()
        {
        }

        public ExportPhoto(Photo photo)
        {
            Ensure.Any.IsNotNull(photo, nameof(photo));

            ContentType = photo.ContentType;
            Hash = photo.Hash;
            Id = photo.Id;
            ProfileId = photo.ProfileId;

            var actual = new byte[photo.Data.Length];

            photo.Data.Position = 0;
            photo.Data.Read(actual, 0, actual.Length);

            Data = actual;
        }

        public string ContentType { get; set; }

        public byte[] Data { get; set; }

        public string Hash { get; set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}