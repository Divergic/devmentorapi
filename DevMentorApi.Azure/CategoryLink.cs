namespace DevMentorApi.Azure
{
    using System;
    using DevMentorApi.Model;

    public class CategoryLink
    {
        public string CategoryName { get; set; }

        public CategoryGroup Group { get; set; }

        public Guid ProfileId { get; set; }
    }
}