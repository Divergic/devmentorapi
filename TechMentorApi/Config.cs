namespace TechMentorApi
{
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class Config
    {
        public AuthenticationConfig Authentication { get; set; }

        public PhotoConfig Photo { get; set; }

        public CacheConfig Cache { get; set; }

        public StorageConfiguration Storage { get; set; }
    }
}