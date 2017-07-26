namespace DevMentorApi.AcceptanceTests
{
    using System;

    public static class ApiLocation
    {
        public static Uri Ping => new Uri(Config.WebsiteAddress, "/ping");
    }
}