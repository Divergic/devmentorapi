namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using Model;

    public interface ICacheManager
    {
        Account GetAccount(string username);

        ICollection<Category> GetCategories();

        ICollection<Guid> GetCategoryLinks(ProfileResultFilter filter);

        Profile GetProfile(Guid id);

        ICollection<ProfileResult> GetProfileResults();

        void RemoveCategories();

        void StoreAccount(Account account);

        void StoreCategories(ICollection<Category> categories);

        void StoreCategoryLinks(ProfileResultFilter filter, ICollection<Guid> links);

        void StoreProfile(Profile profile);

        void StoreProfileResults(ICollection<ProfileResult> results);
    }
}