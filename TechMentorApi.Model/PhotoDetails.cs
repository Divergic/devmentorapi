namespace TechMentorApi.Model
{
    using System;

    public class PhotoDetails
    {
        public string ETag { get; set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}