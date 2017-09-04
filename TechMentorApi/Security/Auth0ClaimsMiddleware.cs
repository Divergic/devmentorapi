namespace TechMentorApi.Security
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Model;

    public class Auth0ClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public Auth0ClaimsMiddleware(RequestDelegate next)
        {
            Ensure.That(next, nameof(next)).IsNotNull();

            _next = next;
        }

        public async Task Invoke(HttpContext context, ILogger<Auth0ClaimsMiddleware> logger)
        {
            Ensure.That(context, nameof(context)).IsNotNull();
            Ensure.That(logger, nameof(logger)).IsNotNull();

            if (context.Request.Method != "OPTIONS")
            {
                ConvertClaims(context, logger);
            }

            await _next.Invoke(context).ConfigureAwait(false);
        }

        private static void ConvertClaims(
            HttpContext context,
            ILogger logger)
        {
            var identity = context.User?.Identity as ClaimsIdentity;

            if (identity == null)
            {
                logger.LogDebug("No claims identity found in the HTTP context, skipping converting the role claims");

                return;
            }

            if (identity.IsAuthenticated == false)
            {
                logger.LogDebug("User is not authenticated, skipping converting the role claims");

                return;
            }

            var matchingClaims = identity.Claims.Where(x => x.Type == ClaimType.Auth0Roles);

            foreach (var matchingClaim in matchingClaims)
            {
                var role = matchingClaim.Value;

                var newClaim = new Claim(ClaimType.Role, role);

                identity.AddClaim(newClaim);
                identity.RemoveClaim(matchingClaim);
            }
        }
    }
}