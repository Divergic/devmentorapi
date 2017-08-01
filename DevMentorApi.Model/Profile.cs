namespace DevMentorApi.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Profile
    {
        public string About { get; set; }

        public Guid AccountId { get; set; }

        public DateTimeOffset? BannedAt { get; set; }

        public int? BirthYear { get; set; }

        public ICollection<string> Languages { get; }
        public ICollection<Skill> Skills { get; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string Gender { get; set; }

        public string GitHubUsername { get; set; }

        [Required]
        public string LastName { get; set; }

        [EnumDataType(typeof(ProfileStatus))]
        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public string TwitterUsername { get; set; }

        public string Website { get; set; }
    }
}