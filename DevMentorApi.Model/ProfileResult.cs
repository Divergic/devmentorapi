namespace DevMentorApi.Model
{
    using System;

    public class ProfileResult
    {
        public int? BirthYear { get; set; }

        public string FirstName { get; set; }

        public string Gender { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }
        
        public int? YearStartedInTech { get; set; }
    }
}