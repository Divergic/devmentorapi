namespace DevMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using DevMentorApi.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    public class ProfileAdapter : EntityAdapter<Profile>
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None
        };

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

        protected override void WriteAdditionalProperty(IDictionary<string, EntityProperty> properties, PropertyInfo propertyInfo, object propertyValue)
        {
            if (IsJsonStorage(propertyInfo))
            {
                var data = JsonConvert.SerializeObject(propertyValue, _settings);

                base.WriteAdditionalProperty(properties, propertyInfo, data);
            }
            else
            {
                base.WriteAdditionalProperty(properties, propertyInfo, propertyValue);
            }
        }

        protected override void ReadAdditionalProperty(PropertyInfo propertyInfo, EntityProperty propertyValue)
        {
            if (IsJsonStorage(propertyInfo))
            {
                var data = JsonConvert.DeserializeObject(propertyValue.StringValue, propertyInfo.PropertyType);

                propertyInfo.SetValue(Value, data);
            }
            else
            {
                base.ReadAdditionalProperty(propertyInfo, propertyValue);
            }
        }

        private static bool IsJsonStorage(MemberInfo property)
        {
            if (property.Name == nameof(Profile.Languages))
            {
                return true;
            }

            if (property.Name == nameof(Profile.Skills))
            {
                return true;
            }

            return false;
        }
    }
}