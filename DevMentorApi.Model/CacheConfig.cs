namespace DevMentorApi.Model
{
    using System;

    public class CacheConfig : ICacheConfig
    {
        public TimeSpan AccountExpiration
        {
            get
            {
                if (AccountExpirationInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(AccountExpirationInSeconds);
            }
        }

        public int AccountExpirationInSeconds { get; set; }

        public TimeSpan ProfileExpiration
        {
            get
            {
                if (ProfileExpirationInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(ProfileExpirationInSeconds);
            }
        }

        public int ProfileExpirationInSeconds { get; set; }
    }
}