namespace TechMentorApi.Business.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IAvatarCommand
    {
        Task<AvatarDetails> CreateAvatar(Avatar avatar, CancellationToken cancellationToken);
    }
}