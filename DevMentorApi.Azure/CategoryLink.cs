namespace DevMentorApi.Azure
{
    using System;
    using DevMentorApi.Model;

    public class CategoryLink
    {
        public CategoryGroup CategoryGroup { get; set; }

        public string CategoryName { get; set; }

        public Guid ProfileId { get; set; }
    }
}