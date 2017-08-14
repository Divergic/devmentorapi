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

        public TimeSpan CategoriesExpiration
        {
            get
            {
                if (CategoriesExpirationInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(CategoriesExpirationInSeconds);
            }
        }

        public int CategoriesExpirationInSeconds { get; set; }

        public TimeSpan CategoryLinksExpiration
        {
            get
            {
                if (CategoryLinksExpirationInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(CategoryLinksExpirationInSeconds);
            }
        }

        public int CategoryLinksExpirationInSeconds { get; set; }

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

        public TimeSpan ProfileResultsExpiration
        {
            get
            {
                if (ProfileResultsExpirationInSeconds == default(int))
                {
                    return TimeSpan.FromMinutes(5);
                }

                return TimeSpan.FromSeconds(ProfileResultsExpirationInSeconds);
            }
        }

        public int ProfileResultsExpirationInSeconds { get; set; }
    }
}