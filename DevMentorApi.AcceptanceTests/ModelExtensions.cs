namespace DevMentorApi.AcceptanceTests
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Model;

    public static class ModelExtensions
    {
        public static async Task<Profile> Save(this Profile profile,
            ILogger logger = null,
            Account account = null)
        {
            var address = ApiLocation.UserProfile;

            // Assumption here is that this is a new profile and the account does not exist yet
            // The account will be created implicitly via auto-registration
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

        public static async Task<NewCategory> Save(this NewCategory category,
            ILogger logger = null)
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;
            
            await Client.Post(address, logger, category, administrator).ConfigureAwait(false);

            return category;
        }
    }
}