namespace DevMentorApi.AcceptanceTests
{
    using System;
    using Microsoft.Extensions.Configuration;

    public static class Config
    {
        public const string AzureConnectionString = "UseDevelopmentStorage=true";

        static Config()
        {
            var config = new ConfigurationBuilder().AddJsonFile("testSettings.json").Build().Get<ConfigWrapper>();

            if (config.Website != null)
            {
                WebsiteAddress = new Uri(config.Website, UriKind.RelativeOrAbsolute);
            }
        }

        public static Uri WebsiteAddress
        {
            get;
        }
    }

    public class ConfigWrapper
    {
        public string Website
        {
            get;
            set;
        }
    }
}