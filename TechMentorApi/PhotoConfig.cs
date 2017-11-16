namespace TechMentorApi
{
    using TechMentorApi.Model;

    public class PhotoConfig : IPhotoConfig
    {
        public int MaxHeight { get; set; }

        public int MaxLength { get; set; }

        public int MaxWidth { get; set; }
    }
}