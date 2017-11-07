namespace TechMentorApi.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using TechMentorApi.Model;

    public static class ModelExtensions
    {
        public static Profile ClearCategories(this Profile profile)
        {
            profile.Skills.Clear();
            profile.Languages.Clear();
            profile.Gender = null;

            return profile;
        }

        public static async Task<Profile> Save(this Profile profile, ILogger logger = null, Account account = null)
        {
            var address = ApiLocation.AccountProfile;

            // If account is null then this will be invoked with a new account
            // This is a one-time usage for testing because the caller will not have access 
            // to the account context for any additional calls
            // If additional calls are required for the same account context then pass an account in and reuse it
            var identity = ClaimsIdentityFactory.Build(account, profile);

            await Client.Put(address, logger, profile, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            var actual = await Client.Get<Profile>(address, logger, identity).ConfigureAwait(false);

            if (profile.BannedAt != null)
            {
                var profileUri = ApiLocation.ProfileFor(actual.Id);
                var administrator = ClaimsIdentityFactory.Build().AsAdministrator();

                // Use an admin to cancel the profile
                await Client.Delete(profileUri, logger, administrator).ConfigureAwait(false);

                actual.BannedAt = profile.BannedAt;
            }

            return actual;
        }

        public static async Task<List<Profile>> Save(this IEnumerable<Profile> profiles, ILogger logger = null)
        {
            var results = new List<Profile>();

            foreach (var profile in profiles)
            {
                var storedProfile = await Save(profile, logger).ConfigureAwait(false);

                results.Add(storedProfile);
            }

            return results;
        }

        public static async Task<NewCategory> Save(this NewCategory category, ILogger logger = null,
            ClaimsIdentity administrator = null)
        {
            if (administrator == null)
            {
                administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            }

            var address = ApiLocation.Categories;

            await Client.Post(address, logger, category, administrator).ConfigureAwait(false);

            return category;
        }

        public static async Task SaveAllCategories(this Profile profile, ILogger logger = null,
            Account account = null)
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();

            var tasks = new List<Task>();

            if (profile.Gender != null)
            {
                tasks.Add(new NewCategory {Group = CategoryGroup.Gender, Name = profile.Gender}.Save(logger,
                    administrator));
            }

            foreach (var language in profile.Languages)
            {
                tasks.Add(new NewCategory {Group = CategoryGroup.Language, Name = language}
                    .Save(logger, administrator));
            }

            foreach (var skill in profile.Skills)
            {
                tasks.Add(new NewCategory {Group = CategoryGroup.Skill, Name = skill.Name}.Save(logger, administrator));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}