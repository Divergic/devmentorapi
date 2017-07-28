namespace DevMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.Extensions.Caching.Memory;

    public class ProfileManager : IProfileManager
    {
        private readonly IMemoryCache _cache;
        private readonly ICacheConfig _config;
        private readonly IProfileStore _store;

        public ProfileManager(IProfileStore store, IMemoryCache cache, ICacheConfig config)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _store = store;
            _cache = cache;
            _config = config;
        }

        public Task BanProfile(Guid accountId, DateTimeOffset bannedAt, CancellationToken cancellationToken)
        {
            Ensure.That(accountId, nameof(accountId)).IsNotEmpty();
            
            // Update the cache of profiles to remove this profile so that it does not appear in the public directory
            // In the short term, we will just update the profile and rely on the public search to filter out banned profiles

            return _store.BanProfile(accountId, bannedAt, cancellationToken);
        }

        public async Task<Profile> GetProfile(Guid accountId, CancellationToken cancellationToken)
        {
            Profile profile;

            var cacheKey = BuildCacheKey(accountId);

            if (_cache.TryGetValue(cacheKey, out profile))
            {
                return profile;
            }

            profile = await _store.GetProfile(accountId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return null;
            }

            // Cache this account for lookup later
            var cacheEntry = _cache.CreateEntry(cacheKey);

            cacheEntry.SlidingExpiration = _config.ProfileExpiration;
            cacheEntry.Value = profile;

            return profile;
        }

        private static string BuildCacheKey(Guid accountId)
        {
// The cache key has a prefix to partition this type of object just in case there is a key collision with another object type
            var cacheKey = "Profile|" + accountId;
            return cacheKey;
        }
    }
}