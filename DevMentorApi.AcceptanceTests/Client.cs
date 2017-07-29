namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class Client
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task Get(
            Uri address,
            Account account = null,
            Profile authenticatedProfile = null,
            ILogger logger = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await GetInternal(address, account, authenticatedProfile, logger, expectedCode, cancellationToken);
        }

        public static async Task<T> Get<T>(
            Uri address,
            Account account = null,
            Profile authenticatedProfile = null,
            ILogger logger = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = await GetInternal(
                address,
                account,
                authenticatedProfile,
                logger,
                expectedCode,
                cancellationToken);

            var value = JsonConvert.DeserializeObject<T>(content);

            return value;
        }

        public static async Task Head(
            Uri address,
            ILogger logger = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Head, address);
            var response = await _client.SendAsync(request, cancellationToken);

            logger?.LogInformation(
                "{0}: {1} - {2}",
                Config.WebsiteAddress.MakeRelativeUri(address),
                response.StatusCode,
                response.ReasonPhrase);

            response.StatusCode.Should().Be(expectedCode);
        }

        private static async Task<string> GetInternal(
            Uri address,
            Account account,
            Profile authenticatedProfile,
            ILogger logger,
            HttpStatusCode expectedCode,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, address);

            if (account != null ||
                authenticatedProfile != null)
            {
                var token = TokenFactory.GenerateToken(account, authenticatedProfile);

                logger?.LogInformation("Bearer: {0}", token);

                request.Headers.Add("Authorization", "Bearer " + token);
            }

            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            logger?.LogInformation(
                "{0}: {1} - {2}",
                Config.WebsiteAddress.MakeRelativeUri(address),
                response.StatusCode,
                response.ReasonPhrase);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            logger?.LogInformation(content);

            response.StatusCode.Should().Be(expectedCode);

            return content;
        }
    }
}