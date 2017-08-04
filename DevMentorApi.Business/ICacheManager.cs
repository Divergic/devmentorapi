namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using DevMentorApi.Model;

    public interface ICacheManager
    {
        Account GetAccount(string username);

        ICollection<Category> GetCategories();

        Profile GetProfile(Guid id);

        void RemoveCategories();

        void StoreAccount(Account account);

        void StoreCategories(ICollection<Category> categories);

        void StoreProfile(Profile profile);
    }
}