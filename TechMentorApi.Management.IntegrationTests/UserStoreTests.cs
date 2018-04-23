using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TechMentorApi.Management.IntegrationTests
{
    public class UserStoreTests
    {
        [Fact]
        public async Task IgnoresWhenUserNotFoundTest()
        {
            var username = "google -oauth2|123123123123123123";  // This user does not exist
            var client = new HttpClient();
            var config = Config.Auth0Management;

            var sut = new UserStore(config, client);

            await sut.DeleteUser(username, CancellationToken.None).ConfigureAwait(false);
        }
    }
}