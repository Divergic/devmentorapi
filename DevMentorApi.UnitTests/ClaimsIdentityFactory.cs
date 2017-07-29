namespace DevMentorApi.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using DevMentorApi.Model;
    using EnsureThat;
    using ModelBuilder;

    public static class ClaimsIdentityFactory
    {
        public static ClaimsPrincipal BuildPrincipal(Account account = null)
        {
            if (account == null)
            {
                account = Model.Create<Account>();
            }

            var identities = new List<ClaimsIdentity>
            {
                BuildIdentity(account)
            };

            return new ClaimsPrincipal(identities);
        }

        public static ClaimsIdentity SetAccountId(this ClaimsIdentity identity, Guid accountId)
        {
            Ensure.That(identity, nameof(identity)).IsNotNull();

            if (identity.HasClaim(x => x.Type == ClaimType.AccountId))
            {
                var claim = identity.Claims.Single(x => x.Type == ClaimType.AccountId);

                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(ClaimType.AccountId, accountId.ToString()));

            return identity;
        }

        public static ClaimsPrincipal SetAccountId(this ClaimsPrincipal principal, Guid accountId)
        {
            Ensure.That(principal, nameof(principal)).IsNotNull();

            var identity = principal.Identities.First();

            if (identity.HasClaim(x => x.Type == ClaimType.AccountId))
            {
                var claim = identity.Claims.Single(x => x.Type == ClaimType.AccountId);

                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(ClaimType.AccountId, accountId.ToString()));

            return principal;
        }

        private static ClaimsIdentity BuildIdentity(Account account = null)
        {
            if (account == null)
            {
                account = Model.Create<Account>();
            }

            IList<Claim> identities = new List<Claim>
            {
                new Claim(ClaimType.AccountId, account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Username)
            };

            return new ClaimsIdentity(identities, "Testing");
        }
    }
}