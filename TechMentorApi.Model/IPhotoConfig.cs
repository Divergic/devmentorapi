namespace TechMentorApi.Model
{
    public interface IPhotoConfig
    {
        int MaxHeight { get; set; }

        int MaxLength { get; set; }

        int MaxWidth { get; set; }
    }
}