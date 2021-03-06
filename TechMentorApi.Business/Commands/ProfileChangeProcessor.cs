﻿namespace TechMentorApi.Business.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class ProfileChangeProcessor : IProfileChangeProcessor
    {
        private readonly ICategoryCache _cache;
        private readonly ICategoryStore _categoryStore;
        private readonly IEventTrigger _eventTrigger;
        private readonly ICategoryLinkStore _linkStore;
        private readonly IProfileCache _profileCache;
        private readonly IProfileStore _profileStore;

        public ProfileChangeProcessor(
            IProfileStore profileStore,
            ICategoryStore categoryStore,
            ICategoryLinkStore linkStore,
            IEventTrigger eventTrigger,
            IProfileCache profileCache,
            ICategoryCache cache)
        {
            Ensure.Any.IsNotNull(profileStore, nameof(profileStore));
            Ensure.Any.IsNotNull(categoryStore, nameof(categoryStore));
            Ensure.Any.IsNotNull(linkStore, nameof(linkStore));
            Ensure.Any.IsNotNull(profileCache, nameof(profileCache));
            Ensure.Any.IsNotNull(cache, nameof(cache));
            Ensure.Any.IsNotNull(eventTrigger, nameof(eventTrigger));

            _profileStore = profileStore;
            _categoryStore = categoryStore;
            _linkStore = linkStore;
            _eventTrigger = eventTrigger;
            _profileCache = profileCache;
            _cache = cache;
        }

        public async Task Execute(Profile profile, ProfileChangeResult changes, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));
            Ensure.Any.IsNotNull(changes, nameof(changes));

            if (changes.CategoryChanges.Count > 0)
            {
                await UpdateCategories(profile, changes, cancellationToken).ConfigureAwait(false);
            }

            if (changes.ProfileChanged)
            {
                await UpdateProfile(profile, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateCategories(
            Profile profile,
            ProfileChangeResult changes,
            CancellationToken cancellationToken)
        {
            var cacheItemsToRemove = new List<ProfileFilter>();
            
            // Get the current categories
            var categories =
                (await _categoryStore.GetAllCategories(cancellationToken).ConfigureAwait(false)).FastToList();

            var categoryTasks = new List<Task>();

            // Write all category link changes
            foreach (var categoryChange in changes.CategoryChanges)
            {
                // Get the category
                var category = categories.FirstOrDefault(
                    x => x.Group == categoryChange.CategoryGroup && x.Name.Equals(
                             categoryChange.CategoryName,
                             StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    // We haven't seen this category before
                    category = new Category
                    {
                        Group = categoryChange.CategoryGroup,
                        LinkCount = 0,
                        Name = categoryChange.CategoryName,
                        Reviewed = false,
                        Visible = false
                    };

                    // Trigger a notification that a new category is being added to the system
                    var newCategoryTriggerTask = _eventTrigger.NewCategory(category, cancellationToken);

                    categoryTasks.Add(newCategoryTriggerTask);

                    categories.Add(category);
                }

                if (categoryChange.ChangeType == CategoryLinkChangeType.Add)
                {
                    // This is a new link between the profile and the category
                    category.LinkCount++;
                }
                else
                {
                    // We are removing the link between the profile and the category
                    category.LinkCount--;
                }

                var change = new CategoryLinkChange
                {
                    ChangeType = categoryChange.ChangeType,
                    ProfileId = profile.Id
                };

                // Store the link update
                var categoryLinkTask = _linkStore.StoreCategoryLink(
                    category.Group,
                    category.Name,
                    change,
                    cancellationToken);

                categoryTasks.Add(categoryLinkTask);

                var filter = new ProfileFilter
                {
                    CategoryGroup = category.Group,
                    CategoryName = category.Name
                };

                    cacheItemsToRemove.Add(filter);

                // Update the category data
                var categoryTask = _categoryStore.StoreCategory(category, cancellationToken);

                categoryTasks.Add(categoryTask);
            }

            // Run all the category task changes together
            await Task.WhenAll(categoryTasks).ConfigureAwait(false);

            UpdateCacheStore(categories, cacheItemsToRemove);
        }

        private void UpdateCacheStore(ICollection<Category> categories, IEnumerable<ProfileFilter> filters)
        {
            // We either made changes to the category link count or a category was added
            // Store the new category list in the cache
            _cache.StoreCategories(categories);

            foreach (var filter in filters)
            {
                // Clear the cache to ensure that this profile is reflected in the category links on subsequent calls
                _cache.RemoveCategoryLinks(filter);

                // Remove the category from the cache
                _cache.RemoveCategory(filter);
            }

        }

        private async Task UpdateProfile(Profile profile, CancellationToken cancellationToken)
        {
            var storeTask = _profileStore.StoreProfile(profile, cancellationToken);
            var triggerTask = _eventTrigger.ProfileUpdated(profile, cancellationToken);

            await Task.WhenAll(storeTask, triggerTask).ConfigureAwait(false);

            _profileCache.StoreProfile(profile);
        }
    }
}