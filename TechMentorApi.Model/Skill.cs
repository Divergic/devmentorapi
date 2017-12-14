namespace TechMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;
    using EnsureThat;

    public class Skill
    {
        public Skill()
        {
        }

        public Skill(Skill source)
        {
            Ensure.That(source, nameof(source)).IsNotNull();

            Level = source.Level;
            Name = source.Name;
            YearLastUsed = source.YearLastUsed;
            YearStarted = source.YearStarted;
        }

        [EnumDataType(typeof(SkillLevel))]
        public SkillLevel Level { get; set; }

        [Required]
        public string Name { get; set; }

        [ValidPastYear(1989)]
        [GreaterOrEqualTo(nameof(YearStarted))]
        public int? YearLastUsed { get; set; }

        [ValidPastYear(1989)]
        public int? YearStarted { get; set; }
    }
}