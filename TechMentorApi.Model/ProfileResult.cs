namespace TechMentorApi.Model
{
    using System;
    using EnsureThat;

    public class ProfileResult
    {
        public ProfileResult()
        {
        }

        public ProfileResult(ProfileResult profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            BirthYear = profile.BirthYear;
            FirstName = profile.FirstName;
            Gender = profile.Gender;
            Id = profile.Id;
            LastName = profile.LastName;
            PhotoHash = profile.PhotoHash;
            PhotoId = profile.PhotoId;
            Status = profile.Status;
            TimeZone = profile.TimeZone;
            YearStartedInTech = profile.YearStartedInTech;
        }

        public ProfileResult(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            BirthYear = profile.BirthYear;
            FirstName = profile.FirstName;
            Gender = profile.Gender;
            Id = profile.Id;
            LastName = profile.LastName;
            PhotoHash = profile.PhotoHash;
            PhotoId = profile.PhotoId;
            Status = profile.Status;
            TimeZone = profile.TimeZone;
            YearStartedInTech = profile.YearStartedInTech;
        }

        public int? BirthYear { get; set; }

        public string FirstName { get; set; }

        public string Gender { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public string PhotoHash { get; set; }

        public Guid? PhotoId { get; set; }

        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public int? YearStartedInTech { get; set; }
    }
}