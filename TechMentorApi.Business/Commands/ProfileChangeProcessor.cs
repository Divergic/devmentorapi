namespace TechMentorApi.Business.Commands
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
        private readonly ICacheManager _cache;
        private readonly ICategoryStore _categoryStore;
        private readonly ICategoryLinkStore _linkStore;
        private readonly IProfileStore _profileStore;

        public ProfileChangeProcessor(
            IProfileStore profileStore,
            ICategoryStore categoryStore,
            ICategoryLinkStore linkStore,
            ICacheManager cache)
        {
            Ensure.Any.IsNotNull(profileStore, nameof(profileStore));
            Ensure.Any.IsNotNull(categoryStore, nameof(categoryStore));
            Ensure.Any.IsNotNull(linkStore, nameof(linkStore));
            Ensure.Any.IsNotNull(cache, nameof(cache));

            _profileStore = profileStore;
            _categoryStore = categoryStore;
            _linkStore = linkStore;
            _cache = cache;
        }

        public async Task Execute(Profile profile, ProfileChangeResult changes, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(profile, nameof(profile));
            Ensure.Any.IsNotNull(changes, nameof(changes));

            if (changes.CategoryChanges.Count > 0)
            {
                // Get the current categories
                var categories = (await _categoryStore.GetAllCategories(cancellationToken).ConfigureAwait(false))
                    .FastToList();

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
                    var categoryLinkTask = _linkStore.StoreCategoryLink(category.Group, category.Name, change, cancellationToken);

                    categoryTasks.Add(categoryLinkTask);

                    var filter = new ProfileFilter
                    {
                        CategoryGroup = category.Group,
                        CategoryName = category.Name
                    };

                    // Clear the cache to ensure that this profile is reflected in the category links on subsequent calls
                    _cache.RemoveCategoryLinks(filter);

                    // Update the category data
                    var categoryTask = _categoryStore.StoreCategory(category, cancellationToken);

                    categoryTasks.Add(categoryTask);
                }

                // Run all the category task changes together
                await Task.WhenAll(categoryTasks).ConfigureAwait(false);

                // We either made changes to the category link count or a category was added
                // Store the new category list in the cache
                _cache.StoreCategories(categories);
            }

            if (changes.ProfileChanged)
            {
                await _profileStore.StoreProfile(profile, cancellationToken).ConfigureAwait(false);

                _cache.StoreProfile(profile);
            }
        }
    }
}