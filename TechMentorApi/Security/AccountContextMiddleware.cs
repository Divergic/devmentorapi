namespace TechMentorApi.Security
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Business;
    using TechMentorApi.Model;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class AccountContextMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountContextMiddleware(RequestDelegate next)
        {
            Ensure.That(next, nameof(next)).IsNotNull();

            _next = next;
        }

        public async Task Invoke(HttpContext context, IAccountManager manager, ILogger<AccountContextMiddleware> logger)
        {
            Ensure.That(context, nameof(context)).IsNotNull();
            Ensure.That(manager, nameof(manager)).IsNotNull();
            Ensure.That(logger, nameof(logger)).IsNotNull();

            if (context.Request.Method != "OPTIONS")
            {
                await ApplyAccountContextAsync(context, manager, logger, CancellationToken.None).ConfigureAwait(false);
            }

            await _next.Invoke(context).ConfigureAwait(false);
        }

        private static async Task ApplyAccountContextAsync(
            HttpContext context,
            IAccountManager manager,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var identity = context.User?.Identity as ClaimsIdentity;

            if (identity == null)
            {
                logger.LogDebug("No claims identity found in the HTTP context, skipping setting the Id claim");

                return;
            }

            if (identity.IsAuthenticated == false)
            {
                logger.LogDebug("User is not authenticated, skipping setting the Id claim");

                return;
            }

            if (identity.HasClaim(x => x.Type == ClaimType.ProfileId))
            {
                logger.LogDebug("The identity on the request already has the Id claim");

                return;
            }

            var email = identity.GetClaimValue<string>(ClaimType.Email);
            var firstName = identity.GetClaimValue<string>(ClaimType.GivenName);
            var lastName = identity.GetClaimValue<string>(ClaimType.Surname);
            var user = new User(identity.Name)
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var account = await manager.GetAccount(user, cancellationToken).ConfigureAwait(false);

            if (account == null)
            {
                return;
            }

            if (identity.HasClaim(x => x.Type == ClaimType.ProfileId))
            {
                return;
            }

            var profileIdClaim = new Claim(ClaimType.ProfileId, account.Id.ToString());

            identity.AddClaim(profileIdClaim);
        }
    }
}