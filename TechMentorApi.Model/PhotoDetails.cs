namespace TechMentorApi.Model
{
    using System;

    public class PhotoDetails
    {
        public string Hash { get; set; }

        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }
    }
}