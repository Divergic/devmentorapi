namespace TechMentorApi.Model
{
    using System;
    using System.IO;
    using EnsureThat;

    public class Photo : IDisposable
    {
        private bool _disposed;

        public Photo()
        {
        }

        public Photo(Photo source, Stream data)
        {
            Ensure.Any.IsNotNull(source, nameof(source));
            Ensure.Any.IsNotNull(data, nameof(data));

            ContentType = source.ContentType;
            Data = data;
            Hash = source.Hash;
            Id = source.Id;
            ProfileId = source.ProfileId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Data?.Dispose();

            _disposed = true;
        }

        public string ContentType { get; set; }

        public Stream Data { get; set; }

        public string Hash { get; set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}