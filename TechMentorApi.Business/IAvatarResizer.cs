namespace TechMentorApi.Business
{
    using TechMentorApi.Model;

    public interface IAvatarResizer
    {
        Avatar Resize(Avatar avatar, int maxHeight, int maxWidth);
    }
}