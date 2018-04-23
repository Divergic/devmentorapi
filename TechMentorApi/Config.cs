namespace TechMentorApi
{
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class Config
    {
        public Auth0ManagementConfig Auth0Management { get; set; }
        public AuthenticationConfig Authentication { get; set; }
        public CacheConfig Cache { get; set; }
        public PhotoConfig Photo { get; set; }
        public StorageConfiguration Storage { get; set; }
    }
}