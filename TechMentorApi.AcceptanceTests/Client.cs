namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
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

        public static async Task Delete(
            Uri address,
            ILogger logger = null,
            ClaimsIdentity identity = null,
            HttpStatusCode expectedCode = HttpStatusCode.NoContent,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, address);

            WriteLogHeader(logger, request);

            if (identity != null)
            {
                var token = TokenFactory.GenerateToken(identity);

                logger?.LogInformation("Identity: {0}", identity.Name);
                logger?.LogInformation("Bearer: {0}", token);

                request.Headers.Add("Authorization", "Bearer " + token);
            }

            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            WriteLogFooter(logger, request, response);

            response.StatusCode.Should().Be(expectedCode);
        }

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
            var data = await GetInternal(address, logger, identity, expectedCode, cancellationToken)
                .ConfigureAwait(false);

            if (typeof(T) == typeof(byte[]))
            {
                logger?.LogInformation("Get returned {0} bytes", data.Length);

                return (T) (object) data;
            }

            var content = Encoding.UTF8.GetString(data);

            logger?.LogInformation(content);

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

            WriteLogFooter(logger, request, response);

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

        public static async Task<Tuple<Uri, T>> PostFile<T>(
            Uri address,
            ILogger logger = null,
            byte[] data = null,
            ClaimsIdentity identity = null,
            string contentType = "image/jpeg",
            HttpStatusCode expectedCode = HttpStatusCode.Created,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, address);

            WriteLogHeader(logger, request);

            if (identity != null)
            {
                var token = TokenFactory.GenerateToken(identity);

                logger?.LogInformation("Identity: {0}", identity.Name);
                logger?.LogInformation("Bearer: {0}", token);

                request.Headers.Add("Authorization", "Bearer " + token);
            }

            var content = new MultipartFormDataContent();

            var fileExtension = contentType.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

            if (data != null)
            {
                var arrayContent = new ByteArrayContent(data);
                var fileName = "\"" + Guid.NewGuid().ToString("N") + "." + fileExtension + "\"";

                arrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                content.Add(arrayContent, "file", fileName);

                logger?.LogInformation("Post data: {0} bytes", data.Length);
            }

            request.Content = content;

            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            WriteLogFooter(logger, request, response);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            logger?.LogInformation(responseContent);

            response.StatusCode.Should().Be(expectedCode);

            var location = response.Headers.Location;

            var value = JsonConvert.DeserializeObject<T>(responseContent);

            return new Tuple<Uri, T>(location, value);
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

            WriteLogHeader(logger, request);

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

            WriteLogFooter(logger, request, response);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            logger?.LogInformation(content);

            response.StatusCode.Should().Be(expectedCode);

            return content;
        }

        private static async Task<byte[]> GetInternal(
            Uri address,
            ILogger logger,
            ClaimsIdentity identity,
            HttpStatusCode expectedCode,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, address);

            WriteLogHeader(logger, request);

            if (identity != null)
            {
                var token = TokenFactory.GenerateToken(identity);

                logger?.LogInformation("Identity: {0}", identity.Name);
                logger?.LogInformation("Bearer: {0}", token);

                request.Headers.Add("Authorization", "Bearer " + token);
            }

            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            WriteLogFooter(logger, request, response);

            var content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            response.StatusCode.Should().Be(expectedCode);

            return content;
        }

        private static void WriteLogFooter(ILogger logger, HttpRequestMessage request, HttpResponseMessage response)
        {
            logger?.LogInformation(
                "{0} {1}: {2} - {3}\r\n\r\n",
                request.Method,
                request.RequestUri,
                response.StatusCode,
                response.ReasonPhrase);
        }

        private static void WriteLogHeader(ILogger logger, HttpRequestMessage request)
        {
            logger?.LogInformation(
                "Start {0} {1}",
                request.Method,
                request.RequestUri);
        }
    }
}