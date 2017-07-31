namespace DevMentorApi.Model
{
    using System;

    public interface ICacheConfig
    {
        TimeSpan AccountExpiration { get; }
        TimeSpan CategoriesExpiration { get; }
        TimeSpan ProfileExpiration { get; }
    }
}