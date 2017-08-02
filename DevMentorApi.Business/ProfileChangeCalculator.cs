namespace DevMentorApi.Business
{
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

            var profileChanged = false;
            var result = new ProfileChangeResult();

            if (original.Gender != updated.Gender)
            {
                if (string.IsNullOrWhiteSpace(original.Gender) == false)
                {
                    // There was previous a gender assigned
                    result.AddChange(CategoryGroup.Gender, original.Gender, CategoryLinkChangeType.Remove);

                    profileChanged = true;
                }

                if (string.IsNullOrWhiteSpace(updated.Gender) == false)
                {
                    result.AddChange(CategoryGroup.Gender, updated.Gender, CategoryLinkChangeType.Add);

                    profileChanged = true;
                }
            }

            // Check for changes to languages

            // Check for changes to skills

            if (profileChanged == false)
            {
                result.ProfileChanged = HasProfileChanged(original, updated);
            }

            return result;
        }

        private static bool HasProfileChanged(Profile original, Profile updated)
        {
            if (original.About != updated.About)
            {
                return true;
            }

            if (original.BirthYear != updated.BirthYear)
            {
                return true;
            }

            if (original.Email != updated.Email)
            {
                return true;
            }

            if (original.FirstName != updated.FirstName)
            {
                return true;
            }

            if (original.GitHubUsername != updated.GitHubUsername)
            {
                return true;
            }

            if (original.LastName != updated.LastName)
            {
                return true;
            }

            if (original.Status != updated.Status)
            {
                return true;
            }

            if (original.TimeZone != updated.TimeZone)
            {
                return true;
            }

            if (original.TwitterUsername != updated.TwitterUsername)
            {
                return true;
            }

            if (original.Website != updated.Website)
            {
                return true;
            }

            if (original.YearStartedInTech != updated.YearStartedInTech)
            {
                return true;
            }

            return false;
        }
    }
}