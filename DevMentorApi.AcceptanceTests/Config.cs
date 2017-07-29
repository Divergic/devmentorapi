namespace DevMentorApi.AcceptanceTests
{
    using System;
    using DevMentorApi.Azure;
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

            Storage = config.Storage;
        }

        public static StorageConfiguration Storage { get; }

        public static Uri WebsiteAddress { get; }
    }

    public class ConfigWrapper
    {
        public StorageConfiguration Storage { get; set; }

        public string Website { get; set; }
    }
}