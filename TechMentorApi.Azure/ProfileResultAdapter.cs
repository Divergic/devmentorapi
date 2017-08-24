namespace TechMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Model;

    public class ProfileResultAdapter : EntityAdapter<ProfileResult>
    {
        public ProfileResultAdapter()
        {
        }

        public ProfileResultAdapter(ProfileResult profile)
            : base(profile)
        {
        }

        public static string BuildPartitionKey(Guid profileId)
        {
            // Use the first character of the Guid for the partition key
            // This will provide up to 16 partitions
            return profileId.ToString().Substring(0, 1);
        }

        public static string BuildRowKey(Guid profileId)
        {
            return profileId.ToString();
        }

        protected override string BuildPartitionKey()
        {
            return BuildPartitionKey(Value.Id);
        }

        protected override string BuildRowKey()
        {
            return BuildRowKey(Value.Id);
        }

        protected override void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            Value.Id = Guid.Parse(RowKey);

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            properties.Remove(nameof(ProfileResult.Id));

            base.WriteValues(properties, operationContext);
        }

        public DateTimeOffset? BannedAt { get; set; }
    }
}