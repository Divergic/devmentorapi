namespace TechMentorApi.Security
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Core;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Model;
    using Properties;

    public class ShieldExceptionMiddleware
    {
        private readonly IResultExecutor _executor;
        private readonly ILogger<ShieldExceptionMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ShieldExceptionMiddleware(
            RequestDelegate next,
            ILogger<ShieldExceptionMiddleware> logger,
            IResultExecutor executor)
        {
            Ensure.Any.IsNotNull(next, nameof(next));
            Ensure.Any.IsNotNull(logger, nameof(logger));
            Ensure.Any.IsNotNull(executor, nameof(executor));

            _next = next;
            _logger = logger;
            _executor = executor;
        }

        public async Task Invoke(HttpContext context)
        {
            Ensure.Any.IsNotNull(context, nameof(context));

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (NotFoundException ex)
            {
                // Something wasn't found deeper in the application stack, return a NotFoundResult
                var result = new ErrorMessageResult(
                    ex.Message,
                    HttpStatusCode.NotFound);

                await _executor.Execute(context, result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(
                        default(EventId),
                        ex,
                        "Failed to shield exception as response headers have already been written.");

                    return;
                }

                var result = new ErrorMessageResult(
                    Resources.WebApi_ExceptionShieldMessage,
                    HttpStatusCode.InternalServerError);

                await _executor.Execute(context, result).ConfigureAwait(false);
            }
        }
    }
}