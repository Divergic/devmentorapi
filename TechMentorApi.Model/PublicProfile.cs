namespace TechMentorApi.Model
{
    using System;
    using EnsureThat;

    public class PublicProfile
    {
        public PublicProfile()
        {
        }

        public PublicProfile(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            About = profile.About;
            Id = profile.Id;
            BirthYear = profile.BirthYear;
            FirstName = profile.FirstName;
            Gender = profile.Gender;
            GitHubUsername = profile.GitHubUsername;
            LastName = profile.LastName;
            Status = profile.Status;
            TimeZone = profile.TimeZone;
            TwitterUsername = profile.TwitterUsername;
            Website = profile.Website;
            YearStartedInTech = profile.YearStartedInTech;
        }

        public string About { get; set; }

        public int? BirthYear { get; set; }

        public string FirstName { get; set; }

        public string Gender { get; set; }

        public string GitHubUsername { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public string TwitterUsername { get; set; }

        public string Website { get; set; }

        public int? YearStartedInTech { get; set; }
    }
}