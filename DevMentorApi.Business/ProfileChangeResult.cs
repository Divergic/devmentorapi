namespace DevMentorApi.Business
{
    using System.Collections.Generic;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;

    public class ProfileChangeResult
    {
        public ProfileChangeResult()
        {
            CategoryChanges = new List<CategoryChange>();
        }

        public void AddChange(CategoryGroup categoryGroup, string categoryName, CategoryLinkChangeType changeType)
        {
            Ensure.That(categoryName, nameof(categoryName)).IsNotNullOrWhiteSpace();

            var change = new CategoryChange
            {
                CategoryGroup = categoryGroup,
                CategoryName = categoryName,
                ChangeType = changeType
            };

            CategoryChanges.Add(change);
        }

        public ICollection<CategoryChange> CategoryChanges { get; }

        public bool ProfileChanged { get; set; }
    }
}