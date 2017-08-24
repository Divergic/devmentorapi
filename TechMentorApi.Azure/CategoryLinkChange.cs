namespace TechMentorApi.Azure
{
    using System;

    public class CategoryLinkChange
    {
        public Guid ProfileId { get; set; }

        public CategoryLinkChangeType ChangeType { get; set; }
    }
}