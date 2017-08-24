namespace TechMentorApi
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Claims;

    public static class ClaimsIdentityExtensions
    {
        public static T GetClaimValue<T>(this ClaimsIdentity identity, string claimType)
        {
            if (identity == null)
            {
                return default(T);
            }

            var claim = identity.FindFirst(x => x.Type == claimType);

            if (claim == null)
            {
                return default(T);
            }

            if (typeof(string) == typeof(T))
            {
                // We can't directly cast string as T even though T is string
                // We need to cast as an object which can then direct cast to T
                // This avoids calculation of type convert information 
                // Additionally, string is a reference type so we don't have boxing and unboxing occuring
                // We are simply copying a memory address for this operation
                object claimValue = claim.Value;

                return (T)claimValue;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFromString(claim.Value);
            }

            var changedValue = Convert.ChangeType(claim.Value, typeof(T), CultureInfo.CurrentCulture);

            if (changedValue == null)
            {
                return default(T);
            }

            // Attempt a forced cast and let the CLR throw the InvalidCastException
            return (T)changedValue;
        }
    }
}