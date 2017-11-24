namespace TechMentorApi.Core
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;

    public class ExceptionMonitorMiddleware
    {
        private readonly ILoggerFactory _logFactory;
        private readonly RequestDelegate _next;

        public ExceptionMonitorMiddleware(RequestDelegate next, ILoggerFactory logFactory)
        {
            Ensure.Any.IsNotNull(next, nameof(next));
            Ensure.Any.IsNotNull(logFactory, nameof(logFactory));

            _next = next;
            _logFactory = logFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            Ensure.Any.IsNotNull(context, nameof(context));

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);

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

            var storageExceptionData = GetStorageExceptionData(exception);

            if (string.IsNullOrWhiteSpace(storageExceptionData) == false)
            {
                log.LogError(eventId, storageExceptionData);
            }

            return Task.CompletedTask;
        }

        private static StorageException FindStorageException(Exception ex)
        {
            var exception = ex;

            if (exception is AggregateException aggregateException)
            {
                exception = aggregateException.Flatten();
            }

            if (exception is StorageException storageException)
            {
                return storageException;
            }

            if (exception.InnerException == null)
            {
                return null;
            }

            return FindStorageException(exception.InnerException);
        }

        private static string GetStorageExceptionData(Exception exception)
        {
            var storageException = FindStorageException(exception);

            if (storageException == null)
            {
                return null;
            }

            var builder = new StringBuilder();

            builder.AppendLine(storageException.Message);

            var information = storageException.RequestInformation.ExtendedErrorInformation;

            if (information == null)
            {
                return builder.ToString();
            }

            builder.AppendLine(information.ErrorCode + " - " + information.ErrorMessage);

            if (information.AdditionalDetails == null)
            {
                return builder.ToString();
            }

            var keys = information.AdditionalDetails.Keys;

            foreach (var key in keys)
            {
                builder.AppendLine(key + " - " + information.AdditionalDetails[key]);
            }

            return builder.ToString();
        }
    }
}