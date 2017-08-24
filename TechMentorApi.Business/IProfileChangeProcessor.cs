namespace TechMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Model;

    public interface IProfileChangeProcessor
    {
        Task Execute(Profile profile, ProfileChangeResult changes, CancellationToken cancellationToken);
    }
}