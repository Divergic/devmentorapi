namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using TechMentorApi.Model;

    public interface IProfileCache
    {
        Profile GetProfile(Guid id);

        ICollection<ProfileResult> GetProfileResults();

        void RemoveProfile(Guid id);

        void StoreProfile(Profile profile);

        void StoreProfileResults(ICollection<ProfileResult> results);
    }
}