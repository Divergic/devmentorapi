namespace DevMentorApi.AcceptanceTests
{
    using System;

    public static class ApiLocation
    {
        public static Uri PublicProfileFor(Guid accountId)
        {
            return new Uri(Config.WebsiteAddress, "/profiles/" + accountId);
        }

        public static Uri Categories => new Uri(Config.WebsiteAddress, "/categories");
        public static Uri Ping => new Uri(Config.WebsiteAddress, "/ping");
        public static Uri Profile => new Uri(Config.WebsiteAddress, "/profile");
    }
}