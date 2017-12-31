namespace TechMentorApi.Azure
{
    using EnsureThat;
    using TechMentorApi.Model;

    public class UpdatedProfileQueue : QueueStore<Profile>, IUpdatedProfileQueue
    {
        public UpdatedProfileQueue(IStorageConfiguration configuration) : base(
            GetConnectionString(configuration),
            "updatedprofiles")
        {
        }

        private static string GetConnectionString(IStorageConfiguration config)
        {
            Ensure.Any.IsNotNull(config, nameof(config));

            return config.ConnectionString;
        }
    }
}