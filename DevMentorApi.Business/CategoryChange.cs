namespace DevMentorApi.Business
{
    using DevMentorApi.Azure;
    using DevMentorApi.Model;

    public class CategoryChange
    {
        public CategoryGroup CategoryGroup { get; set; }

        public string CategoryName { get; set; }

        public CategoryLinkChangeType ChangeType { get; set; }
    }
}