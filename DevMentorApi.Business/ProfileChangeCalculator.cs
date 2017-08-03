namespace DevMentorApi.Business
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using DevMentorApi.Azure;
    using DevMentorApi.Model;
    using EnsureThat;

    public class ProfileChangeCalculator : IProfileChangeCalculator
    {
        public ProfileChangeResult CalculateChanges(Profile original, Profile updated)
        {
            Ensure.That(original, nameof(original)).IsNotNull();
            Ensure.That(updated, nameof(updated)).IsNotNull();
            Ensure.That(original.Id == updated.Id, nameof(Profile.Id)).IsTrue();

            var result = new ProfileChangeResult();

            DetermineCategoryChanges(CategoryGroup.Gender, original.Gender, updated.Gender, result);

            // Check for changes to languages
            DetermineCategoryChanges(CategoryGroup.Language, original.Languages, updated.Languages, result);

            // Check for changes to skills
            var originalSkillNames = original.Skills.Select(x => x.Name).ToList();
            var updatedSkillNames = updated.Skills.Select(x => x.Name).ToList();

            DetermineCategoryChanges(CategoryGroup.Skill, originalSkillNames, updatedSkillNames, result);

            if (result.ProfileChanged == false)
            {
                result.ProfileChanged = HasProfileChanged(original, updated);
            }

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
                    updatedNames.FirstOrDefault(x => x.Equals(updatedName, StringComparison.OrdinalIgnoreCase));

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

        private static bool HasProfileChanged(Profile original, Profile updated)
        {
            Debug.Assert(original != null, "No original provider provided");
            Debug.Assert(updated != null, "No updated provider provided");

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
    }
}