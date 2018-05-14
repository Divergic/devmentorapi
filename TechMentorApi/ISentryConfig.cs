namespace TechMentorApi
{
    public interface ISentryConfig
    {
        string Dsn { get; }

        string Environment { get; }

        string Version { get; }
    }
}