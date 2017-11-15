namespace TechMentorApi.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class UpdatableProfile
    {
        private ICollection<string> _languages;
        private ICollection<Skill> _skills;

        public UpdatableProfile()
        {
            Languages = new List<string>();
            Skills = new List<Skill>();
        }

        public string About { get; set; }

        public int? BirthYear { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string Gender { get; set; }

        public string GitHubUsername { get; set; }

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

        [Required]
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

        [EnumDataType(typeof(ProfileStatus))]
        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public string TwitterUsername { get; set; }

        public string Website { get; set; }

        [ValidPastYear(1989)]
        public int? YearStartedInTech { get; set; }
    }
}