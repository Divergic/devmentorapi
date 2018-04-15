namespace TechMentorApi.Management.IntegrationTests
{
    using Microsoft.Extensions.Configuration;

    public static class Config
    {
        static Config()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("managementsettings.json")
                .AddJsonFile("managementsettings.Development.json", true)
                .Build().Get<ConfigWrapper>();

            Auth0Management = config.Auth0Management;
        }

        public static Auth0ManagementConfig Auth0Management { get; }
    }

    public class ConfigWrapper
    {
        public Auth0ManagementConfig Auth0Management { get; set; }
    }
}