namespace TechMentorApi.Azure
{
    using System;
    using TechMentorApi.Model;

    public class CategoryLink
    {
        public CategoryGroup CategoryGroup { get; set; }

        public string CategoryName { get; set; }

        public Guid ProfileId { get; set; }
    }
}