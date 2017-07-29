namespace DevMentorApi.Core
{
    using System;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class ExceptionMonitorMiddleware
    {
        private readonly ILoggerFactory _logFactory;
        private readonly RequestDelegate _next;

        public ExceptionMonitorMiddleware(RequestDelegate next, ILoggerFactory logFactory)
        {
            Ensure.That(next, nameof(next)).IsNotNull();
            Ensure.That(logFactory, nameof(logFactory)).IsNotNull();

            _next = next;
            _logFactory = logFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            Ensure.That(context, nameof(context)).IsNotNull();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);

                throw;
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception == null)
            {
                return Task.CompletedTask;
            }
            
            var log = _logFactory.CreateLogger(GetType());

            var eventId = new EventId(0);

            log.LogError(eventId, exception, exception.Message);

            return Task.CompletedTask;
        }
    }
}