namespace DevMentorApi.AcceptanceTests
{
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public static class ModelExtensions
    {
        public static async Task<Profile> Save(this Profile profile)
        {
            var address = ApiLocation.Profile;

            // Assumption here is that this is a new profile and the account does not exist yet
            // The account will be created implicitly via auto-registration
            var identity = ClaimsIdentityFactory.Build(null, profile);

            var actual = await Client.Get<Profile>(address, null, identity).ConfigureAwait(false);

            return actual;
        }

        public static async Task<NewCategory> Save(this NewCategory category)
        {
            var administrator = ClaimsIdentityFactory.Build().AsAdministrator();
            var address = ApiLocation.Categories;

            // Ensure that at least one category exists
            await Client.Post(address, null, category, administrator).ConfigureAwait(false);

            return category;
        }
    }
}