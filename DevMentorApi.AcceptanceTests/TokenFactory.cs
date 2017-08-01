namespace DevMentorApi.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Jose;

    public static class TokenFactory
    {
        public static string GenerateToken(ClaimsIdentity identity)
        {
            const string ClientSecret = "qwertyuiopasdfghjklzxcvbnm123456";
            var secretKey = Base64UrlDecode(ClientSecret);
            var issued = DateTime.Now;
            var expire = DateTime.Now.AddHours(10);

            var payload = new Dictionary<string, object>
            {
                {
                    "iss", "http://dev.local/"
                },
                {
                    "aud", "http://localhost:5000/"
                },
                {
                    "sub", identity.Name
                },
                {
                    "iat", ToUnixTime(issued).ToString()
                },
                {
                    "exp", ToUnixTime(expire).ToString()
                }
            };

            foreach (var claim in identity.Claims)
            {
                if (claim.Type == "sub")
                {
                    continue;
                }

                payload.Add(claim.Type, claim.Value);
            }

            var token = JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);

            return token;
        }

        /// <remarks>
        ///     Take from http://stackoverflow.com/a/33113820
        /// </remarks>
        private static byte[] Base64UrlDecode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2:
                    s += "==";
                    break; // Two pad chars
                case 3:
                    s += "=";
                    break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        private static long ToUnixTime(DateTime dateTime)
        {
            return (int)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}