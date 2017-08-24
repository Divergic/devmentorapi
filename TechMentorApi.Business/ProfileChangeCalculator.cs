namespace TechMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using TechMentorApi.Azure;
    using TechMentorApi.Model;
    using EnsureThat;

    public class ProfileChangeCalculator : IProfileChangeCalculator
    {
        public ProfileChangeResult CalculateChanges(Profile original, UpdatableProfile updated)
        {
            Ensure.That(original, nameof(original)).IsNotNull();
            Ensure.That(updated, nameof(updated)).IsNotNull();

            var result = new ProfileChangeResult();

            DetermineCategoryChanges(CategoryGroup.Gender, original.Gender, updated.Gender, result);

            // Check for changes to languages
            DetermineCategoryChanges(CategoryGroup.Language, original.Languages, updated.Languages, result);

            // Check for changes to skills
            var originalSkillNames = original.Skills.Select(x => x.Name).ToList();
            var updatedSkillNames = updated.Skills.Select(x => x.Name).ToList();

            DetermineCategoryChanges(CategoryGroup.Skill, originalSkillNames, updatedSkillNames, result);

            // At this point all category add/remove operations have been determined
            // If the profile is banned then we don't want to create any links to categories
            if (original.BannedAt.HasValue)
            {
                // Either the profile hasn't changed yet or it has and we have category changes to process
                // Wiping out the category changes has the outcome we want by leaving ProfileChanged as is and not allowing any category changes
                result.CategoryChanges.Clear();
            }

            // Only thing remaining is to try to find a change to the profile data outside of categories
            // If no changes found to categories by now, we just need to know whether the profile itself needs to be sent to storage
            if (result.ProfileChanged == false)
            {
                result.ProfileChanged = HasProfileChanged(original, updated);
            }

            if (result.ProfileChanged == false)
            {
                // There haven't been any skills added or removed, but there could be changed to the skill information
                // Search for changes to skill metadata
                result.ProfileChanged = HaveSkillsChanged(original.Skills, updated.Skills);
            }

            return result;
        }

        public ProfileChangeResult RemoveAllCategoryLinks(Profile original)
        {
            Ensure.That(original, nameof(original)).IsNotNull();

            var result = new ProfileChangeResult();

            // Remove gender link
            DetermineCategoryChanges(CategoryGroup.Gender, original.Gender, null, result);

            // Check for changes to languages
            var emptyCategoryNames = new List<string>();

            // Remove all language links
            DetermineCategoryChanges(CategoryGroup.Language, original.Languages, emptyCategoryNames, result);

            var originalSkillNames = original.Skills.Select(x => x.Name).ToList();

            // Remove all skill links
            DetermineCategoryChanges(CategoryGroup.Skill, originalSkillNames, emptyCategoryNames, result);

            return result;
        }

        private static void DetermineCategoryChanges(
            CategoryGroup categoryGroup,
            ICollection<string> originalNames,
            ICollection<string> updatedNames,
            ProfileChangeResult result)
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