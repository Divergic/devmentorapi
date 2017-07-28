namespace DevMentorApi.Model
{
    using System;

    public interface ICacheConfig
    {
        TimeSpan AccountExpiration { get; }
        TimeSpan ProfileExpiration { get; }
    }
}