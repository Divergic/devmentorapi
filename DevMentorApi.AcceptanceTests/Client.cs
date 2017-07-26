namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;

    public static class Client
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task Head(
            Uri address,
            ILogger logger = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Head, address);
            var response = await _client.SendAsync(request, cancellationToken);

            logger?.LogInformation("{0}: {1} - {2}", Config.WebsiteAddress.MakeRelativeUri(address), response.StatusCode, response.ReasonPhrase);

            response.StatusCode.Should().Be(expectedCode);
        }
    }
}