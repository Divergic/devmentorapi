namespace TechMentorApi
{
    using System.Security.Claims;
    using System.Security.Principal;

    public static class IdentityExtensions
    {
        public static T GetClaimValue<T>(this IIdentity identity, string claimType)
        {
            if (identity == null)
            {
                return default(T);
            }

            var claimsIdentity = identity as ClaimsIdentity;

            if (claimsIdentity == null)
            {
                return default(T);
            }

            return claimsIdentity.GetClaimValue<T>(claimType);
        }
    }
}