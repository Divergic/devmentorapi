namespace TechMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using TechMentorApi.Model;
    using TechMentorApi.Security;

    public static class ClaimsIdentityFactory
    {
        public static ClaimsIdentity AsAdministrator(this ClaimsIdentity identity)
        {
            var claim = new Claim("role", Role.Administrator);

            identity.AddClaim(claim);

            return identity;
        }

        public static ClaimsIdentity Build(Account account = null, UpdatableProfile profile = null)
        {
            var claims = new List<Claim>();

            var username = Guid.NewGuid().ToString();

            if (account != null)
            {
                username = account.Username;
            }

            if (username.IndexOf("|", StringComparison.OrdinalIgnoreCase) == -1)
            {
                username = "local|" + username;
            }

            AddClaim(claims, "sub", username);
            AddClaim(claims, "email", profile?.Email);
            AddClaim(claims, "givenName", profile?.FirstName);
            AddClaim(claims, "surname", profile?.LastName);

            var identity = new ClaimsIdentity(claims, "local", "sub", "role");

            return identity;
        }

        private static void AddClaim(ICollection<Claim> claims, string claimType, string claimValue)
        {
            if (string.IsNullOrWhiteSpace(claimValue))
            {
                return;
            }

            var claim = new Claim(claimType, claimValue);

            claims.Add(claim);
        }
    }
}