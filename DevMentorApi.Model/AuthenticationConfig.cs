namespace DevMentorApi.Model
{
    using System;

    public class AuthenticationConfig : IAuthenticationConfig
    {
        public int AccountCacheTimeoutInSeconds
        {
            get;
            set;
        }

        public string Audience
        {
            get;
            set;
        }

        public string Authority
        {
            get;
            set;
        }

        public bool RequireHttps
        {
            get;
            set;
        }

        public string SecretKey
        {
            get;
            set;
        }

        public TimeSpan AccountCacheTtl
        {
            get
            {
                if (AccountCacheTimeoutInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(AccountCacheTimeoutInSeconds);
            }
        }
    }
}