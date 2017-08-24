namespace TechMentorApi.Azure
{
    using System.Collections.Generic;
    using TechMentorApi.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AccountAdapter : EntityAdapter<Account>
    {
        public AccountAdapter()
        {
        }

        public AccountAdapter(Account account) : base(account)
        {
        }

        protected override string BuildPartitionKey()
        {
            return Value.Provider;
        }

        protected override string BuildRowKey()
        {
            return Value.Username;
        }

        protected override void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            Value.Provider = PartitionKey;
            Value.Username = RowKey;

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            properties.Remove(nameof(Account.Provider));
            properties.Remove(nameof(Account.Username));

            base.WriteValues(properties, operationContext);
        }
    }
}