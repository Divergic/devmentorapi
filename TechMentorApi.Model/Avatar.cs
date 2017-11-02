namespace TechMentorApi.Model
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public class Avatar : IDisposable
    {
        private bool _disposed;

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