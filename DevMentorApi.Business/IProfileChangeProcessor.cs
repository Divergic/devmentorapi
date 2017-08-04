namespace DevMentorApi.Business
{
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IProfileChangeProcessor
    {
        Task Execute(Profile profile, ProfileChangeResult changes, CancellationToken cancellationToken);
    }
}