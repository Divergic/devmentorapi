namespace TechMentorApi.AcceptanceTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using TechMentorApi.Model;
    using Microsoft.Extensions.Logging;

    public static class ModelExtensions
    {
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

        public static async Task<NewCategory> Save(this NewCategory category, ILogger logger = null)
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            await Client.Post(address, logger, category, administrator).ConfigureAwait(false);

            return category;
        }
    }
}