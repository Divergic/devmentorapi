namespace TechMentorApi.Azure
{
    using System;
    using EnsureThat;
    using TechMentorApi.Model;

    public class NewCategoryQueue : QueueStore<Category>, INewCategoryQueue
    {
        public NewCategoryQueue(IStorageConfiguration configuration) : base(
            GetConnectionString(configuration),
            "newcategories")
        {
        }

        protected override string SerializeMessage(Category message)
        {
            var messageContent = message.Group + Environment.NewLine + message.Name;

            return messageContent;
        }

        private static string GetConnectionString(IStorageConfiguration config)
        {
            Ensure.Any.IsNotNull(config, nameof(config));

            return config.ConnectionString;
        }
    }
}