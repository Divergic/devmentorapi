namespace TechMentorApi.Business.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using EnsureThat;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class ProfileChangeCalculator : IProfileChangeCalculator
    {
        public ProfileChangeResult CalculateChanges(Profile original, UpdatableProfile updated)
        {
            Ensure.That(original, nameof(original)).IsNotNull();
            Ensure.That(updated, nameof(updated)).IsNotNull();

            var result = new ProfileChangeResult();
            var canUpdateCategoryLinks = true;

            if (original.Status == ProfileStatus.Hidden &&
                updated.Status == ProfileStatus.Hidden)
            {
                // We don't calculate any changes to category links for a hidden profiles that are still hidden
                // If we do look for category links to determine if the profile should be saved, we can exit once at least one is found
                canUpdateCategoryLinks = false;
            }
            else if (original.BannedAt != null)
            {
                // We don't calculate any changes to category links for a banned profile
                // If we do look for category links to determine if the profile should be saved, we can exit once at least one is found
                canUpdateCategoryLinks = false;
            }

            if (original.Status != ProfileStatus.Hidden &&
                updated.Status == ProfileStatus.Hidden)
            {
                // The profile is being hidden
                // Remove all the existing category links
                // We will remove all the links from the original profile
                // This is because if there are any changes from the original profile to the updated profile, 
                // they aren't stored yet in the links so we don't care
                DetermineAllCategoryRemovalChanges(original, result);
            }
            else if (original.Status == ProfileStatus.Hidden &&
                     updated.Status != ProfileStatus.Hidden)
            {
                // The profile is being displayed after being hidden
                // We need to add all the links to the updated profile
                DetermineAllCategoryAddChanges(updated, result);
            }
            else
            {
                // Find the category changes between the original and updated profiles
                DetermineCategoryChanges(original, updated, canUpdateCategoryLinks, result);
            }

            if (result.ProfileChanged == false)
            {
                result.ProfileChanged = HasProfileChanged(original, updated);
            }

            if (result.ProfileChanged == false)
            {
                // The profile properties have not changed
                // We haven't checked for category items added or removed yet, but we also need to check
                // if any skills have been changed
                // Search for changes to skill metadata
                result.ProfileChanged = HaveSkillsChanged(original.Skills, updated.Skills);
            }

            if (canUpdateCategoryLinks == false)
            {
                // We may have calculated category link changes in order to figure out if the profile should be saved
                // but we are not going make any category link changes so we need to clear them out
                result.CategoryChanges.Clear();
            }

            return result;
        }

        public ProfileChangeResult RemoveAllCategoryLinks(UpdatableProfile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            var result = new ProfileChangeResult();

            DetermineAllCategoryRemovalChanges(profile, result);

            return result;
        }

        private static void DetermineAllCategoryAddChanges(UpdatableProfile profile, ProfileChangeResult result)
        {
            // Remove gender link
            DetermineCategoryChanges(CategoryGroup.Gender, null, profile.Gender, result);

            // Check for changes to languages
            var emptyCategoryNames = new List<string>();

            // Remove all language links
            DetermineCategoryChanges(CategoryGroup.Language, emptyCategoryNames, profile.Languages, true, result);

            var skillNames = profile.Skills.Select(x => x.Name).ToList();

            // Remove all skill links
            DetermineCategoryChanges(CategoryGroup.Skill, emptyCategoryNames, skillNames, true, result);
        }

        private static void DetermineAllCategoryRemovalChanges(UpdatableProfile profile, ProfileChangeResult result)
        {
            // Remove gender link
            DetermineCategoryChanges(CategoryGroup.Gender, profile.Gender, null, result);

            // Check for changes to languages
            var emptyCategoryNames = new List<string>();

            // Remove all language links
            DetermineCategoryChanges(CategoryGroup.Language, profile.Languages, emptyCategoryNames, true, result);

            var skillNames = profile.Skills.Select(x => x.Name).ToList();

            // Remove all skill links
            DetermineCategoryChanges(CategoryGroup.Skill, skillNames, emptyCategoryNames, true, result);
        }

        private static void DetermineCategoryChanges(Profile original, UpdatableProfile updated,
            bool findAllCategoryChanges, ProfileChangeResult result)
        {
            // Find the category changes
            DetermineCategoryChanges(CategoryGroup.Gender, original.Gender, updated.Gender, result);

            if (findAllCategoryChanges == false &&
                result.ProfileChanged)
            {
                // We only need to find a single change which we have
                return;
            }

            // Check for changes to languages
            DetermineCategoryChanges(CategoryGroup.Language, original.Languages, updated.Languages,
                findAllCategoryChanges, result);

            if (findAllCategoryChanges == false &&
                result.ProfileChanged)
            {
                // We only need to find a single change which we have
                return;
            }

            // Check for changes to skills
            var originalSkillNames = original.Skills.Select(x => x.Name).ToList();
            var updatedSkillNames = updated.Skills.Select(x => x.Name).ToList();

            DetermineCategoryChanges(CategoryGroup.Skill, originalSkillNames, updatedSkillNames, findAllCategoryChanges,
                result);
        }

        private static void DetermineCategoryChanges(CategoryGroup categoryGroup, ICollection<string> originalNames,
            ICollection<string> updatedNames, bool findAllCategoryChanges, ProfileChangeResult result)
        {
            foreach (var originalName in originalNames)
            {
                var matchingUpdatedName =
                    updatedNames.FirstOrDefault(x => x.Equals(originalName, StringComparison.OrdinalIgnoreCase));

                if (matchingUpdatedName == null)
                {
                    // This category has been removed from profile
                    result.AddChange(categoryGroup, originalName, CategoryLinkChangeType.Remove);

                    result.ProfileChanged = true;
                }

                if (findAllCategoryChanges == false &&
                    result.ProfileChanged)
                {
                    // We only need to find a single change which we have
                    return;
                }
            }

            foreach (var updatedName in updatedNames)
            {
                var matchingOriginalName =
                    originalNames.FirstOrDefault(x => x.Equals(updatedName, StringComparison.OrdinalIgnoreCase));

                if (matchingOriginalName == null)
                {
                    // This category has been added to the profile
                    result.AddChange(categoryGroup, updatedName, CategoryLinkChangeType.Add);

                    result.ProfileChanged = true;
                }

                if (findAllCategoryChanges == false &&
                    result.ProfileChanged)
                {
                    // We only need to find a single change which we have
                    return;
                }
            }
        }

        private static void DetermineCategoryChanges(
            CategoryGroup categoryGroup,
            string originalName,
            string updatedName,
            ProfileChangeResult result)
        {
            // Categories are stored with case insensitive keys to avoid duplicates
            // So we don't care if just the case has changed for the category link
            if (HasStringChanged(originalName, updatedName, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(originalName) == false)
                {
                    // There was previous a gender assigned
                    result.AddChange(categoryGroup, originalName, CategoryLinkChangeType.Remove);

                    result.ProfileChanged = true;
                }

                if (string.IsNullOrWhiteSpace(updatedName) == false)
                {
                    // There is a new gender assigned
                    result.AddChange(categoryGroup, updatedName, CategoryLinkChangeType.Add);

                    result.ProfileChanged = true;
                }
            }
        }

        private static bool HasIntChanged(int? original, int? updated)
        {
            if (original == null &&
                updated == null)
            {
                // Nothing has changed
                return false;
            }

            if (original == null ||
                updated == null)
            {
                // One of the values has a value and the other doesn't
                return true;
            }

            // Both strings have a value
            if (original == updated)
            {
                return false;
            }

            return true;
        }

        private static bool HasProfileChanged(Profile original, UpdatableProfile updated)
        {
            Debug.Assert(original != null, "No original profile provided");
            Debug.Assert(updated != null, "No updated profile provided");

            if (HasStringChanged(original.About, updated.About))
            {
                return true;
            }

            if (HasIntChanged(original.BirthYear, updated.BirthYear))
            {
                return true;
            }

            if (HasStringChanged(original.Email, updated.Email))
            {
                return true;
            }

            if (HasStringChanged(original.FirstName, updated.FirstName))
            {
                return true;
            }

            if (HasStringChanged(original.GitHubUsername, updated.GitHubUsername))
            {
                return true;
            }

            if (HasStringChanged(original.LastName, updated.LastName))
            {
                return true;
            }

            if (original.Status != updated.Status)
            {
                return true;
            }

            if (HasStringChanged(original.TimeZone, updated.TimeZone))
            {
                return true;
            }

            if (HasStringChanged(original.TwitterUsername, updated.TwitterUsername))
            {
                return true;
            }

            if (HasStringChanged(original.Website, updated.Website))
            {
                return true;
            }

            if (HasIntChanged(original.YearStartedInTech, updated.YearStartedInTech))
            {
                return true;
            }

            return false;
        }

        private static bool HasSkillChanged(Skill original, Skill updated)
        {
            Debug.Assert(original != null, "No original skill provided");
            Debug.Assert(updated != null, "No updated skill provided");

            if (original.Level != updated.Level)
            {
                return true;
            }

            if (HasIntChanged(original.YearLastUsed, updated.YearLastUsed))
            {
                return true;
            }

            if (HasIntChanged(original.YearStarted, updated.YearStarted))
            {
                return true;
            }

            return false;
        }

        private static bool HasStringChanged(
            string original,
            string updated,
            StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (string.IsNullOrWhiteSpace(original) &&
                string.IsNullOrWhiteSpace(updated))
            {
                // Nothing has changed
                return false;
            }

            if (string.IsNullOrWhiteSpace(original) ||
                string.IsNullOrWhiteSpace(updated))
            {
                // One of the values has a value and the other doesn't
                return true;
            }

            // Both strings have a value
            if (string.Equals(original, updated, comparisonType))
            {
                return false;
            }

            return true;
        }

        private static bool HaveSkillsChanged(ICollection<Skill> originalSkills, ICollection<Skill> updatedSkills)
        {
            // At this point, all skill categories match so the number of items and their names are equivalent
            foreach (var originalSkill in originalSkills)
            {
                var updatedSkill = updatedSkills.First(
                    x => x.Name.Equals(originalSkill.Name, StringComparison.OrdinalIgnoreCase));

                if (HasSkillChanged(originalSkill, updatedSkill))
                {
                    return true;
                }
            }

            return false;
        }
    }
}