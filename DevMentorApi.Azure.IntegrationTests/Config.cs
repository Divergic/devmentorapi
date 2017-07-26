namespace DevMentorApi.Azure.IntegrationTests
{
    internal static class Config
    {
        private const string TableDataConnectionString = "UseDevelopmentStorage=true";

        public static IStorageConfiguration Storage => new StorageConfiguration
        {
            ConnectionString = TableDataConnectionString
        };
    }
}