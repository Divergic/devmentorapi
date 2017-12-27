namespace TechMentorApi.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using TechMentorApi.Model;

    public class NewCategoryQueue : QueueStore, INewCategoryQueue
    {
        public NewCategoryQueue(IStorageConfiguration configuration)
            : base(GetConnectionString(configuration), "newcategories")
        {
        }

        public Task WriteCategory(Category category, CancellationToken cancellationToken)
        {
            Ensure.Any.IsNotNull(category, nameof(category));

            var message = category.Group + Environment.NewLine + category.Name;

            return WriteMessage(message, null, null, cancellationToken);
        }

        private static string GetConnectionString(IStorageConfiguration config)
        {
            Ensure.Any.IsNotNull(config, nameof(config));

            return config.ConnectionString;
        }
    }
}