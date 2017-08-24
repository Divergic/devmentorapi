namespace TechMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;

    public class NewCategory
    {
        [EnumDataType(typeof(CategoryGroup))]
        public CategoryGroup Group { get; set; }

        [Required]
        public string Name { get; set; }
    }
}