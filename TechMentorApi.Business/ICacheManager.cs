namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using TechMentorApi.Model;

    public interface ICacheManager
    {
        ICollection<Category> GetCategories();

        Category GetCategory(CategoryGroup group, string name);

        ICollection<Guid> GetCategoryLinks(ProfileFilter filter);

        void RemoveCategories();

        void RemoveCategory(Category category);

        void RemoveCategoryLinks(ProfileFilter filter);

        void StoreCategories(ICollection<Category> categories);

        void StoreCategory(Category category);

        void StoreCategoryLinks(ProfileFilter filter, ICollection<Guid> links);
    }
}