namespace TechMentorApi.Model
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;

    public class PublicProfile
    {
        private ICollection<string> _languages;
        private ICollection<Skill> _skills;

        public PublicProfile()
        {
            Languages = new List<string>();
            Skills = new List<Skill>();
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
            Languages = profile.Languages;
            LastName = profile.LastName;
            PhotoHash = profile.PhotoHash;
            PhotoId = profile.PhotoId;
            Skills = profile.Skills;
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

        public ICollection<string> Languages
        {
            get => _languages;
            set
            {
                if (value == null)
                {
                    value = new List<string>();
                }

                _languages = value;
            }
        }

        public string LastName { get; set; }

        public string PhotoHash { get; set; }

        public Guid? PhotoId { get; set; }

        public ICollection<Skill> Skills
        {
            get => _skills;
            set
            {
                if (value == null)
                {
                    value = new List<Skill>();
                }

                _skills = value;
            }
        }

        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public string TwitterUsername { get; set; }

        public string Website { get; set; }

        public int? YearStartedInTech { get; set; }
    }
}