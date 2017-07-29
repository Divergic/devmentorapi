namespace DevMentorApi.AcceptanceTests
{
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public static class ModelExtensions
    {
        public static async Task<Profile> Save(this Profile profile)
        {
            var address = ApiLocation.Profile;

            var actual = await Client.Get<Profile>(address, null, profile).ConfigureAwait(false);

            return actual;
        }
    }
}