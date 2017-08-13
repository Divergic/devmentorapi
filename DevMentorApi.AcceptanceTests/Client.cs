namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class Client
    {
        private static readonly HttpClient _client = new HttpClient();

        public static Task Get(
            Uri address,
            ILogger logger = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetInternal(address, logger, identity, expectedCode, cancellationToken);
        }

        public static async Task<T> Get<T>(
            Uri address,
            ILogger logger = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = await GetInternal(address, logger, identity, expectedCode, cancellationToken).ConfigureAwait(false);

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
            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            logger?.LogInformation(
                "{0}: {1} - {2}",
                Config.WebsiteAddress.MakeRelativeUri(address),
                response.StatusCode,
                response.ReasonPhrase);

            response.StatusCode.Should().Be(expectedCode);
        }

        public static Task Post(
            Uri address,
            ILogger logger = null,
            object model = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.Created,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteBodyInternal(
                address,
                HttpMethod.Post,
                logger,
                model,
                identity,
                expectedCode,
                cancellationToken);
        }

        public static async Task<T> Post<T>(
            Uri address,
            ILogger logger = null,
            object model = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.Created,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = await ExecuteBodyInternal(
                address,
                HttpMethod.Post,
                logger,
                model,
                identity,
                expectedCode,
                cancellationToken).ConfigureAwait(false);

            var value = JsonConvert.DeserializeObject<T>(content);

            return value;
        }

        public static Task Put(
            Uri address,
            ILogger logger = null,
            object model = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExecuteBodyInternal(
                address,
                HttpMethod.Put,
                logger,
                model,
                identity,
                expectedCode,
                cancellationToken);
        }

        public static async Task<T> Put<T>(
            Uri address,
            ILogger logger = null,
            object model = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.OK,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var content = await ExecuteBodyInternal(
                address,
                HttpMethod.Put,
                logger,
                model,
                identity,
                expectedCode,
                cancellationToken).ConfigureAwait(false);

            var value = JsonConvert.DeserializeObject<T>(content);

            return value;
        }

        private static async Task<string> ExecuteBodyInternal(
            Uri address,
            HttpMethod method,
            ILogger logger,
            object model,
            ClaimsIdentity identity,
            HttpStatusCode expectedCode,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, address);

            if (identity != null)
            {
                var token = TokenFactory.GenerateToken(identity);

                logger?.LogInformation("Identity: {0}", identity.Name);
                logger?.LogInformation("Bearer: {0}", token);

                request.Headers.Add("Authorization", "Bearer " + token);
            }

            if (model == null)
            {
                request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            }
            else
            {
                var postData = JsonConvert.SerializeObject(model);

                request.Content = new StringContent(postData, Encoding.UTF8, "application/json");

                logger?.LogInformation("Post data: {0}", postData);
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

        private static async Task<string> GetInternal(
            Uri address,
            ILogger logger,
            ClaimsIdentity identity,
            HttpStatusCode expectedCode,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, address);

            if (identity != null)
            {
                var token = TokenFactory.GenerateToken(identity);

                logger?.LogInformation("Identity: {0}", identity.Name);
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