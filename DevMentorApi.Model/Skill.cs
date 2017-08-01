namespace DevMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;

    public class Skill
    {
        [EnumDataType(typeof(SkillLevel))]
        public SkillLevel Level { get; set; }

        [Required]
        public string Name { get; set; }

        [ValidPastYear(1989)]
        public int? YearLastUsed { get; set; }

        [ValidPastYear(1989)]
        public int? YearStarted { get; set; }
    }
}