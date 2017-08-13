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
            var address = ApiLocation.Profile;

            // Assumption here is that this is a new profile and the account does not exist yet
            // The account will be created implicitly via auto-registration
            var identity = ClaimsIdentityFactory.Build(account, profile);

            await Client.Put(address, logger, profile, identity, HttpStatusCode.NoContent).ConfigureAwait(false);

            var actual = await Client.Get<Profile>(address, logger, identity).ConfigureAwait(false);

            return actual;
        }

        public static async Task<NewCategory> Save(this NewCategory category,
            ILogger logger = null)
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            // Ensure that at least one category exists
            await Client.Post(address, logger, category, administrator).ConfigureAwait(false);

            return category;
        }
    }
}