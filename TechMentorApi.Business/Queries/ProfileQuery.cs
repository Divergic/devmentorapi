namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class ProfileQuery : IProfileQuery
    {
        private readonly ICacheManager _cache;
        private readonly IProfileStore _store;

        public ProfileQuery(
            IProfileStore store,
            ICacheManager cache)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _store = store;
            _cache = cache;
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