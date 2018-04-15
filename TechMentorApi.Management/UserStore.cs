using EnsureThat;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TechMentorApi.Management
{
    public class UserStore : IUserStore
    {
        private readonly HttpMessageInvoker _client;
        private readonly IAuth0ManagementConfig _config;

        public UserStore(IAuth0ManagementConfig config, HttpMessageInvoker client)
        {
            Ensure.Any.IsNotNull(config, nameof(config));
            Ensure.Any.IsNotNull(client, nameof(client));

            _config = config;
            _client = client;
        }

        public async Task DeleteUser(string username, CancellationToken cancellationToken)
        {
            Ensure.String.IsNotNullOrWhiteSpace(username, nameof(username));

            if (_config.IsEnabled == false)
            {
                return;
            }

            var token = await GetAccessToken(cancellationToken).ConfigureAwait(false);

            var resource = "https://" + _config.Tenant + ".auth0.com/api/v2/users/" + username;

            using (var request = new HttpRequestMessage(HttpMethod.Delete, resource))
            {
                request.Headers.Add("Authorization", "Bearer " + token.AccessToken);

                using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode == false)
                    {
                        throw new InvalidOperationException(response.ReasonPhrase);
                    }
                }
            }
        }

        private async Task<TokenResponse> GetAccessToken(CancellationToken cancellationToken)
        {
            var resource = "https://" + _config.Tenant + ".auth0.com/oauth/token";
            var payload = "{\"grant_type\":\"client_credentials\",\"client_id\": \"" + _config.ClientId + "\",\"client_secret\": \"" + _config.ClientSecret + "\",\"audience\": \"" + _config.Audience + "\"}";

            using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, resource))
                {
                    request.Content = content;

                    using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        if (response.IsSuccessStatusCode == false)
                        {
                            throw new InvalidOperationException(response.ReasonPhrase);
                        }

                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        // Get the token
                        var token = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                        return token;
                    }
                }
            }
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }
    }
}