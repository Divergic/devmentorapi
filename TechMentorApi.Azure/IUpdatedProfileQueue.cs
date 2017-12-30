namespace TechMentorApi.Azure
{
    using TechMentorApi.Model;

    public interface IUpdatedProfileQueue : IQueueStore<Profile>
    {
    }
}