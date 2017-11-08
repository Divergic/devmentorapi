namespace TechMentorApi.Security
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Core;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Model;
    using Properties;

    public class OversizeDataExceptionMiddleware
    {
        private readonly IResultExecutor _executor;
        private readonly ILogger<OversizeDataExceptionMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IAvatarConfig _config;

        public OversizeDataExceptionMiddleware(
            RequestDelegate next,
            ILogger<OversizeDataExceptionMiddleware> logger,
            IResultExecutor executor,
            IAvatarConfig config)
        {
            Ensure.That(next, nameof(next)).IsNotNull();
            Ensure.That(logger, nameof(logger)).IsNotNull();
            Ensure.That(executor, nameof(executor)).IsNotNull();
            Ensure.That(config, nameof(config)).IsNotNull();

            _next = next;
            _logger = logger;
            _executor = executor;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            Ensure.That(context, nameof(context)).IsNotNull();

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (InvalidDataException ex)
            {
                var expectedMessage = string.Format(CultureInfo.InvariantCulture,
                    "Multipart body length limit {0} exceeded", _config.MaxLength);

                if (ex.Message.Contains(expectedMessage) == false)
                {
                    throw;
                }

                var maxKilobytes = _config.MaxLength / 1024;
                var resultMessage = string.Format(CultureInfo.InvariantCulture, Resources.Post_PayloadTooLarge,
                    maxKilobytes);

                // Someone has uploaded a file that was too large
                var result = new ErrorMessageResult(resultMessage, HttpStatusCode.BadRequest);

                await _executor.Execute(context, result).ConfigureAwait(false);
            }
        }
    }
}