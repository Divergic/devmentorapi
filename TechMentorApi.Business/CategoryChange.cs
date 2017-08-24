namespace TechMentorApi.Business
{
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class CategoryChange
    {
        public CategoryGroup CategoryGroup { get; set; }

        public string CategoryName { get; set; }

        public CategoryLinkChangeType ChangeType { get; set; }
    }
}