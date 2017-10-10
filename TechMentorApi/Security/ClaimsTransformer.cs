namespace TechMentorApi.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Business;
    using EnsureThat;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Model;

    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly ILogger<ClaimsTransformer> _logger;
        private readonly IAccountManager _manager;

        public ClaimsTransformer(IAccountManager manager,
            ILogger<ClaimsTransformer> logger)
        {
            Ensure.That(manager, nameof(manager)).IsNotNull();
            Ensure.That(logger, nameof(logger)).IsNotNull();

            _manager = manager;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated == false)
            {
                _logger.LogDebug("User is not authenticated, skipping transforming claim");

                return principal;
            }

            var identity = principal.Identities.First();

            MapAuth0Claims(identity);

            await ApplyProfileId(identity).ConfigureAwait(false);

            return principal;
        }

        private static void MapAuth0Claims(ClaimsIdentity identity)
        {
            var matchingClaims = identity.Claims.Where(x => x.Type == ClaimType.Auth0Roles).ToList();
            var claimsToAdd = new List<Claim>();

            for (var index = matchingClaims.Count - 1; index >= 0; index--)
            {
                var matchingClaim = matchingClaims[index];

                var role = matchingClaim.Value;

                var newClaim = new Claim(ClaimType.Role, role);

                claimsToAdd.Add(newClaim);
                identity.RemoveClaim(matchingClaim);
            }

            if (claimsToAdd.Count > 0)
            {
                claimsToAdd.ForEach(identity.AddClaim);
            }
        }


        private async Task ApplyProfileId(ClaimsIdentity identity)
        {
            if (identity.HasClaim(x => x.Type == ClaimType.ProfileId))
            {
                _logger.LogDebug("The identity on the request already has the ProfileId claim");
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

            var account = await _manager.GetAccount(user, CancellationToken.None).ConfigureAwait(false);

            if (account == null)
            {
                return;
            }

            var profileIdClaim = new Claim(ClaimType.ProfileId, account.Id.ToString());

            identity.AddClaim(profileIdClaim);
        }
    }
}