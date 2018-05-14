namespace TechMentorApi
{
    public class SentryConfig : ISentryConfig
    {
        public string Dsn { get; set; }

        public string Environment { get; set; }

        public string Version { get; set; }
    }
}