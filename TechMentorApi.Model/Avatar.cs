namespace TechMentorApi.Model
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public class Avatar
    {
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

        public Stream Data { get; set; }

        public string ETag { get; private set; }

        public string Extension { get; set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}