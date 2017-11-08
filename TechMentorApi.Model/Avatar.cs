namespace TechMentorApi.Model
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using EnsureThat;

    public class Avatar : IDisposable
    {
        private bool _disposed;

        public Avatar()
        {
        }

        public Avatar(Avatar source, Stream data)
        {
            Ensure.That(source, nameof(source)).IsNotNull();
            Ensure.That(data, nameof(data)).IsNotNull();

            ContentType = source.ContentType;
            Data = data;
            ETag = source.ETag;
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

        public void SetETag(string etag)
        {
            if (etag == null)
            {
                ETag = null;
            }
            else
            {
                // Strip any non-alphanumeric character
                var filteredETag = Regex.Replace(etag, "[^a-zA-Z0-9]", string.Empty);

                ETag = filteredETag;
            }
        }

        public string ContentType { get; set; }

        public Stream Data { get; set; }

        public string ETag { get; private set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}