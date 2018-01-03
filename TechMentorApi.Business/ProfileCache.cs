namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;
    using TechMentorApi.Model;

    public class ProfileCache : IProfileCache
    {
        private const string ProfileResultsCacheKey = "ProfileResults";
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;

        public ProfileCache(IMemoryCache cache, ICacheConfig config)
        {
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(config, nameof(config));

            _cache = cache;
            _config = config;
        }

        public Profile GetProfile(Guid id)
        {
            Ensure.Guid.IsNotEmpty(id, nameof(id));

            var cacheKey = BuildProfileCacheKey(id);

            return _cache.Get<Profile>(cacheKey);
        }

        public ICollection<ProfileResult> GetProfileResults()
        {
            return _cache.Get<ICollection<ProfileResult>>(ProfileResultsCacheKey);
        }

        public void RemoveProfile(Guid id)
        {
            Ensure.Guid.IsNotEmpty(id, nameof(id));

            var cacheKey = BuildProfileCacheKey(id);

            _cache.Remove(cacheKey);
        }

        public void StoreProfile(Profile profile)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));

            var cacheKey = BuildProfileCacheKey(profile.Id);

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.ProfileExpiration
            };

            _cache.Set(cacheKey, profile, options);
        }

        public void StoreProfileResults(ICollection<ProfileResult> results)
        {
            Ensure.Any.IsNotNull(results, nameof(results));

            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _config.ProfileResultsExpiration
            };

            _cache.Set(ProfileResultsCacheKey, results, options);
        }

        private static string BuildProfileCacheKey(Guid id)
        {
            // The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            return "Profile|" + id;
        }
    }
}