using System;
using System.Collections.Generic;
using System.Text;

namespace DevMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;

    public class Skill
    {
        [Required]
        public string Name { get; set; }

        [EnumDataType(typeof(SkillLevel))]
        public SkillLevel Level { get; set; }
        
        [ValidPastYear(1989)]
        public int YearStarted { get; set; }
    }

    public enum SkillLevel
    {
        Beginner = 0,
        Intermediate,
        Expert,
        Master  // Really wanted to name this Neo
    }
}
