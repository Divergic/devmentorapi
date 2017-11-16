namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IPhotoCommand
    {
        Task<PhotoDetails> CreatePhoto(Photo photo, CancellationToken cancellationToken);
    }
}