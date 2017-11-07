namespace TechMentorApi
{
    using TechMentorApi.Model;

    public class AvatarConfig : IAvatarConfig
    {
        public int MaxHeight { get; set; }

        public int MaxLength { get; set; }

        public int MaxWidth { get; set; }
    }
}