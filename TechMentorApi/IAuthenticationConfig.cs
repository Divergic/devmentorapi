namespace TechMentorApi
{
    public interface IAuthenticationConfig
    {
        string Audience { get; }

        string Authority { get; }

        bool RequireHttps { get; }

        string SecretKey { get; }
    }
}