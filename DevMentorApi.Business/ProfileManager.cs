namespace DevMentorApi.Business
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using EnsureThat;
    using Model;

    public class ProfileManager : IProfileManager
    {
        private readonly ICacheManager _cache;
        private readonly IProfileChangeCalculator _calculator;
        private readonly IProfileChangeProcessor _processor;
        private readonly IProfileStore _store;

        public ProfileManager(
            IProfileStore store,
            IProfileChangeCalculator calculator,
            IProfileChangeProcessor processor,
            ICacheManager cache)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(calculator, nameof(calculator)).IsNotNull();
            Ensure.That(processor, nameof(processor)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _store = store;
            _calculator = calculator;
            _processor = processor;
            _cache = cache;
        }

        public async Task<Profile> BanProfile(Guid id, DateTimeOffset bannedAt, CancellationToken cancellationToken)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            var profile = await _store.BanProfile(id, bannedAt, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return null;
            }

            // Update the cache for this profile
            _cache.StoreProfile(profile);

            // Remove this profile from the results cache
            UpdateResultsCache(profile);

            // TODO: Update all item links (and link caches) to removed the banned profile, then remove the banned profile from cache
            return profile;
        }

        public Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            return FindProfile(id, cancellationToken);
        }

        public async Task<PublicProfile> GetPublicProfile(Guid id, CancellationToken cancellationToken)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            var profile = await FindProfile(id, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return null;
            }

            if (profile.Status == ProfileStatus.Hidden)
            {
                return null;
            }

            if (profile.BannedAt != null)
            {
                return null;
            }

            return new PublicProfile(profile);
        }

        public async Task UpdateProfile(Guid profileId, UpdatableProfile profile, CancellationToken cancellationToken)
        {
            Ensure.That(profileId, nameof(profileId)).IsNotEmpty();
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var original = await _store.GetProfile(profileId, cancellationToken).ConfigureAwait(false);

            var updated = new Profile(profile)
            {
                Id = original.Id,
                BannedAt = original.BannedAt
            };

            if (original.BannedAt != null)
            {
                // We don't calculate any changes to category links for a banned profile
                await _store.StoreProfile(updated, cancellationToken).ConfigureAwait(false);

                return;
            }

            var changes = _calculator.CalculateChanges(original, profile);

            if (changes.ProfileChanged)
            {
                await _processor.Execute(updated, changes, cancellationToken).ConfigureAwait(false);

                UpdateResultsCache(updated);
            }
        }

        private void UpdateResultsCache(Profile profile)
        {
            // The status has changed such that the visibility of the profile has been changed
            // We need to update cache information for the available profiles
            var results = _cache.GetProfileResults();

            if (results == null)
            {
                // There are no results in the cache so we don't need to update it
                return;
            }

            var cachedResult = results.FirstOrDefault(x => x.Id == profile.Id);
            var cacheRequiresUpdate = false;

            if (cachedResult != null)
            {
                results.Remove(cachedResult);
                cacheRequiresUpdate = true;
            }

            if (profile.Status != ProfileStatus.Hidden
                && profile.BannedAt == null)
            {
                // The profile is visible so the updated profile needs to be added into the results cache
                var newResult = new ProfileResult(profile);

                results.Add(newResult);
                cacheRequiresUpdate = true;
            }

            if (cacheRequiresUpdate)
            {
                _cache.StoreProfileResults(results);
            }
        }

        private async Task<Profile> FindProfile(Guid id, CancellationToken cancellationToken)
        {
            var profile = _cache.GetProfile(id);

            if (profile != null)
            {
                return profile;
            }

            profile = await _store.GetProfile(id, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return null;
            }

            _cache.StoreProfile(profile);

            return profile;
        }
    }
}