namespace TechMentorApi.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using TechMentorApi.Model.Properties;

    public class UpdatableProfile : IValidatableObject
    {
        private ICollection<string> _languages;
        private ICollection<Skill> _skills;

        public UpdatableProfile()
        {
            Languages = new List<string>();
            Skills = new List<Skill>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status != ProfileStatus.Hidden)
            {
                // Check to see if there is consent to have this profile visible
                if (AcceptCoC == false &&
                    AcceptTaC == false)
                {
                    yield return new ValidationResult(Resources.NoConsent_Message,
                        new[] {nameof(AcceptCoC), nameof(AcceptTaC)});
                }
                else if (AcceptCoC == false)
                {
                    yield return new ValidationResult(Resources.NoCocConsent_Message, new[] {nameof(AcceptCoC)});
                }
                else if (AcceptTaC == false)
                {
                    yield return new ValidationResult(Resources.NoTacConsent_Message, new[] {nameof(AcceptTaC)});
                }
            }
        }

        public string About { get; set; }

        public bool AcceptCoC { get; set; }

        public bool AcceptTaC { get; set; }

        public int? BirthYear { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [RegularExpression("^[^\\\\/]*$", ErrorMessageResourceName = "NoSlashAttribute_MessageFormat",
            ErrorMessageResourceType = typeof(Resources))]
        public string Gender { get; set; }

        public string GitHubUsername { get; set; }

        [NoSlash]
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