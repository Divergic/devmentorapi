namespace TechMentorApi
{
    using TechMentorApi.Azure;
    using TechMentorApi.Model;

    public class Config
    {
        public AuthenticationConfig Authentication { get; set; }

        public AvatarConfig Avatar { get; set; }

        public CacheConfig Cache { get; set; }

        public StorageConfiguration Storage { get; set; }
    }
}