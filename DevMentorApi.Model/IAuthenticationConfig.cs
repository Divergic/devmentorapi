namespace DevMentorApi.Model
{
    using System;

    public interface IAuthenticationConfig
    {
        TimeSpan AccountCacheTtl { get; }

        string Audience { get; }

        string Authority { get; }

        bool RequireHttps { get; }

        string SecretKey { get; }
    }
}