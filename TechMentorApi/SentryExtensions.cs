namespace TechMentorApi
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using SharpRaven;
    using SharpRaven.Data;

    public static class SentryExtensions
    {
        public static void UseSentry(this IApplicationBuilder builder, string dsn)
        {
            if (string.IsNullOrWhiteSpace(dsn))
            {
                // No need to put in the middleware if there is no Sentry configuration
                return;
            }

            builder.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var client = context.RequestServices.GetService<IRavenClient>();

                    if (string.IsNullOrWhiteSpace(client?.CurrentDsn?.ProjectID))
                    {
                        // There is no client or no configuration for Sentry
                        throw;
                    }
                    
                    var sentryEvent = new SentryEvent(ex);

                    // This is a workaround while SharpRaven support doesn't quite support asp.net core
                    var user = context?.Request?.HttpContext?.User?.Identity?.Name;

                    if (string.IsNullOrWhiteSpace(user) == false)
                    {
                        sentryEvent.Tags.Add("username", user);
                    }
                    
                    var id = await client.CaptureAsync(sentryEvent).ConfigureAwait(false);

                    if (id != null &&
                        !context.Response.HasStarted)
                    {
                        context.Response.Headers.TryAdd("X-Sentry-Id", id);
                    }

                    throw;
                }
            });
        }
    }
}