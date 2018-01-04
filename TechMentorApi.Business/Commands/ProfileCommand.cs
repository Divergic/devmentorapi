namespace TechMentorApi.Business.Commands
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class ProfileCommand : IProfileCommand
    {
        private readonly IProfileCache _cache;
        private readonly IProfileChangeCalculator _calculator;
        private readonly IProfileChangeProcessor _processor;
        private readonly IProfileStore _store;

        public ProfileCommand(
            IProfileStore store,
            IProfileChangeCalculator calculator,
            IProfileChangeProcessor processor,
            IProfileCache cache)
        {
            Ensure.Any.IsNotNull(store, nameof(store));
            Ensure.Any.IsNotNull(calculator, nameof(calculator));
            Ensure.Any.IsNotNull(processor, nameof(processor));
            Ensure.Any.IsNotNull(cache, nameof(cache));

            _store = store;
            _calculator = calculator;
            _processor = processor;
            _cache = cache;
        }

        public async Task<Profile> BanProfile(Guid id, DateTimeOffset bannedAt, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(id, nameof(id));

            var profile = await _store.BanProfile(id, bannedAt, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return null;
            }

            var linkChanges = _calculator.RemoveAllCategoryLinks(profile);

            if (linkChanges.CategoryChanges.Count > 0)
            {
                await _processor.Execute(profile, linkChanges, cancellationToken).ConfigureAwait(false);
            }

            // Remove the profile from cache 
            _cache.RemoveProfile(profile.Id);

            // Remove this profile from the results cache
            UpdateResultsCache(profile);

            return profile;
        }

        public async Task UpdateProfile(Guid profileId, UpdatableProfile profile, CancellationToken cancellationToken)
        {
            Ensure.Guid.IsNotEmpty(profileId, nameof(profileId));
            Ensure.Any.IsNotNull(profile, nameof(profile));

            var original = await _store.GetProfile(profileId, cancellationToken).ConfigureAwait(false);

            var changes = _calculator.CalculateChanges(original, profile);

            if (changes.ProfileChanged)
            {
                var updated = new Profile(profile)
                {
                    Id = original.Id,
                    BannedAt = original.BannedAt
                };

                await _processor.Execute(updated, changes, cancellationToken).ConfigureAwait(false);

                UpdateResultsCache(updated);
            }
        }

        private void UpdateResultsCache(Profile profile)
        {
            // We need to update cache information for the publicly visible profiles
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
                // We found this profile in the cache, the profile has been updated or is hidden or banned
                // We need to start by removing the existing profile from the cache
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
    }
}