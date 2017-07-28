namespace DevMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using DevMentorApi.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class ProfileAdapter : EntityAdapter<Profile>
    {
        public ProfileAdapter()
        {
        }

        public ProfileAdapter(Profile profile) : base(profile)
        {
        }

        public static string BuildPartitionKey(Guid accountId)
        {
            // Use the first character of the Guid for the partition key
            // This will provide up to 16 partitions
            return accountId.ToString().Substring(0, 1);
        }

        public static string BuildRowKey(Guid accountId)
        {
            return accountId.ToString();
        }

        protected override string BuildPartitionKey()
        {
            return BuildPartitionKey(Value.AccountId);
        }

        protected override string BuildRowKey()
        {
            return BuildRowKey(Value.AccountId);
        }

        protected override void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            Value.AccountId = Guid.Parse(RowKey);

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            properties.Remove(nameof(Profile.AccountId));

            base.WriteValues(properties, operationContext);
        }
    }
}