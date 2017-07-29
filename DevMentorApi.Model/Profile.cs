namespace DevMentorApi.Model
{
    using System;

    public class Profile
    {
        public Guid AccountId { get; set; }

        public DateTimeOffset? BannedAt { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ProfileStatus Status { get; set; }
    }
}