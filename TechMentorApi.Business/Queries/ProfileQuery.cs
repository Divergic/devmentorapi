namespace TechMentorApi.Business.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class ProfileQuery : IProfileQuery
    {
        private readonly ICacheManager _cache;
        private readonly ICategoryQuery _query;
        private readonly IProfileStore _store;

        public ProfileQuery(
            IProfileStore store,
            ICacheManager cache,
            ICategoryQuery query)
        {
            Ensure.That(store, nameof(store)).IsNotNull();
            Ensure.That(cache, nameof(cache)).IsNotNull();
            Ensure.That(query, nameof(query)).IsNotNull();

            _store = store;
            _cache = cache;
            _query = query;
        }

        public Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            return FindProfile(id, cancellationToken);
        }

        public async Task<PublicProfile> GetPublicProfile(Guid id, CancellationToken cancellationToken)
        {
            Ensure.That(id, nameof(id)).IsNotEmpty();

            var profileTask = FindProfile(id, cancellationToken);
            var categoriesTask = _query.GetCategories(ReadType.VisibleOnly, cancellationToken);

            // We will get both the visible categories and profile here at the same time
            // If the profile is not found, hidden or banned then this is wasted effort getting the category
            // These two scenarios are unlikely however so better overall to not get these two synchronously
            // Also, the categories are likely to be cached such that this will be a quick call even if it is redundant
            await Task.WhenAll(profileTask, categoriesTask).ConfigureAwait(false);

            var profile = profileTask.Result;

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

            // Use a copy constructor before removing unapproved categories to ensure that changes don't corrupt the reference type stored in the cache
            var publicProfile = new PublicProfile(profile);

            var categories = categoriesTask.Result;

            // We want to split the categories into their groups
            // This will make it more efficient to enumerate through the categories by their groups against the profile
            var categoryGroups = SplitCategoryGroups(categories);

            RemoveUnapprovedCategories(publicProfile, categoryGroups);

            return publicProfile;
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

        private void RemoveUnapprovedCategories(PublicProfile profile,
            IDictionary<CategoryGroup, IList<string>> categoryGroups)
        {
            var approvedGenders = categoryGroups[CategoryGroup.Gender];

            if (profile.Gender != null &&
                approvedGenders.Contains(profile.Gender.ToUpperInvariant()) == false)
            {
                // The gender is not an approved category
                profile.Gender = null;
            }

            // Take a copy of the languages so we can change the source while enumerating
            // ToList will copy the values for us
            var approvedLanguages = categoryGroups[CategoryGroup.Language];
            var profileLanguages = profile.Languages.ToList();

            foreach (var profileLanguage in profileLanguages)
            {
                if (approvedLanguages.Contains(profileLanguage.ToUpperInvariant()) == false)
                {
                    profile.Languages.Remove(profileLanguage);
                }
            }

            // Take a copy of the skills so we can change the source while enumerating
            // ToList will copy the values for us
            var approvedSkills = categoryGroups[CategoryGroup.Skill];
            var profileSkills = profile.Skills.ToList();

            foreach (var profileSkill in profileSkills)
            {
                if (approvedSkills.Contains(profileSkill.Name.ToUpperInvariant()) == false)
                {
                    profile.Skills.Remove(profileSkill);
                }
            }
        }

        private IDictionary<CategoryGroup, IList<string>> SplitCategoryGroups(IEnumerable<Category> categories)
        {
            var genders = new List<string>();
            var skills = new List<string>();
            var languages = new List<string>();

            foreach (var category in categories)
            {
                if (category.Group == CategoryGroup.Gender)
                {
                    genders.Add(category.Name.ToUpperInvariant());
                }
                else if (category.Group == CategoryGroup.Language)
                {
                    languages.Add(category.Name.ToUpperInvariant());
                }
                else if (category.Group == CategoryGroup.Skill)
                {
                    skills.Add(category.Name.ToUpperInvariant());
                }
            }

            return new Dictionary<CategoryGroup, IList<string>>
            {
                {CategoryGroup.Gender, genders},
                {CategoryGroup.Skill, skills},
                {CategoryGroup.Language, languages}
            };
        }
    }
}