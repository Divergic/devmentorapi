namespace TechMentorApi.Business
{
    using TechMentorApi.Model;

    public interface IPhotoResizer
    {
        Photo Resize(Photo photo, int maxHeight, int maxWidth);
    }
}