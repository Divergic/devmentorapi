namespace TechMentorApi.ViewModels
{
    using TechMentorApi.Model;
    using EnsureThat;

    public class PublicCategory
    {
        public PublicCategory()
        {
        }

        public PublicCategory(Category category)
        {
            Ensure.Any.IsNotNull(category, nameof(category));

            Group = category.Group;
            LinkCount = category.LinkCount;
            Name = category.Name;
        }

        public CategoryGroup Group { get; set; }

        public int LinkCount { get; set; }

        public string Name { get; set; }
    }
}