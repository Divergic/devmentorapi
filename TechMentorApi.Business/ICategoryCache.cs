﻿namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using TechMentorApi.Model;

    public interface ICategoryCache
    {
        ICollection<Category> GetCategories();

        Category GetCategory(CategoryGroup group, string name);

        ICollection<Guid> GetCategoryLinks(ProfileFilter filter);

        void RemoveCategories();

        void RemoveCategory(CategoryGroup group, string name);

        void RemoveCategoryLinks(ProfileFilter filter);

        void StoreCategories(ICollection<Category> categories);

        void StoreCategory(Category category);

        void StoreCategoryLinks(ProfileFilter filter, ICollection<Guid> links);
    }
}