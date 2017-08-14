namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using EnsureThat;
    using Model;

    public class ProfileSearchManager : IProfileSearchManager
    {
        private readonly ICacheManager _cache;
        private readonly ICategoryLinkStore _linkStore;
        private readonly IProfileStore _profileStore;

        public ProfileSearchManager(IProfileStore profileStore, ICategoryLinkStore linkStore, ICacheManager cache)
        {
            Ensure.That(profileStore, nameof(profileStore)).IsNotNull();
            Ensure.That(linkStore, nameof(linkStore)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();

            _profileStore = profileStore;
            _linkStore = linkStore;
            _cache = cache;
        }

        public async Task<IEnumerable<ProfileResult>> GetProfileResults(ICollection<ProfileResultFilter> filters,
            CancellationToken cancellationToken)
        {
            var results = await LoadResults(cancellationToken).ConfigureAwait(false);

            if (filters == null)
            {
                return results;
            }

            if (filters.Count == 0)
            {
                return results;
            }

            // Load the category links for each filter
            var matchingProfiles = await FilterProfiles(filters, cancellationToken).ConfigureAwait(false);

            return from x in results
                join y in matchingProfiles
                on x.Id equals y
                select x;
        }

        private async Task<IEnumerable<Guid>> FilterProfiles(ICollection<ProfileResultFilter> filters,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task<IEnumerable<Guid>>>();

            foreach (var filter in filters)
            {
                var task = GetCategoryLinks(filter, cancellationToken);

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            // The same id can be associated with many categories so we need to remove the duplicates
            return results.SelectMany(x => x).Distinct();
        }

        private async Task<IEnumerable<Guid>> GetCategoryLinks(ProfileResultFilter filter,
            CancellationToken cancellationToken)
        {
            var cachedLinks = _cache.GetCategoryLinks(filter);

            if (cachedLinks != null)
            {
                return cachedLinks;
            }

            var storedLinks = (await _linkStore
                    .GetCategoryLinks(filter.CategoryGroup, filter.CategoryName, cancellationToken)
                    .ConfigureAwait(false))
                .ToList();

            var links = (from x in storedLinks
                select x.ProfileId).ToList();

            _cache.StoreCategoryLinks(filter, links);

            return links;
        }

        private async Task<IEnumerable<ProfileResult>> LoadResults(CancellationToken cancellationToken)
        {
            var cachedResults = _cache.GetProfileResults();

            if (cachedResults != null)
            {
                return cachedResults;
            }

            var storeResults = await _profileStore.GetProfileResults(cancellationToken).ConfigureAwait(false);

            // Store the results before caching them
            // Order by available first, highest number of years in tech then oldest by age
            var orderedResults = from x in storeResults
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ??
                                                                                  DateTimeOffset.UtcNow.Year
                select x;

            var finalResults = orderedResults.ToList();

            _cache.StoreProfileResults(finalResults);

            return finalResults;
        }
    }
}