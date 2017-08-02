namespace DevMentorApi.Azure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface ICategoryLinkStore
    {
        Task<IEnumerable<CategoryLink>> GetCategoryLinks(
            CategoryGroup categoryGroup,
            string categoryName,
            CancellationToken cancellationToken);

        Task StoreCategoryLinks(
            CategoryGroup categoryGroup,
            string categoryName,
            IEnumerable<CategoryLinkChange> changes,
            CancellationToken cancellationToken);
    }
}