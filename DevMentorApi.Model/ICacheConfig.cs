namespace DevMentorApi.Model
{
    using System;

    public interface ICacheConfig
    {
        TimeSpan AccountExpiration { get; }

        TimeSpan CategoriesExpiration { get; }

        TimeSpan CategoryLinksExpiration { get; }

        TimeSpan ProfileExpiration { get; }

        TimeSpan ProfileResultsExpiration { get; }
    }
}