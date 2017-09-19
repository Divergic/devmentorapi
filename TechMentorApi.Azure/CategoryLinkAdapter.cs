namespace TechMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TechMentorApi.Model;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class CategoryLinkAdapter : EntityAdapter<CategoryLink>
    {
        public CategoryLinkAdapter()
        {
        }

        public CategoryLinkAdapter(CategoryLink category) : base(category)
        {
        }

        public static string BuildPartitionKey(CategoryGroup categoryGroup, string categoryName)
        {
            Ensure.That(categoryName).IsNotNullOrWhiteSpace();
            
            // We need to Base64 encode the category name to handle #/\ characters in the names
            var invariantName = categoryName.ToUpperInvariant();
            var bytes = Encoding.UTF8.GetBytes(invariantName);

            var encodedName = Convert.ToBase64String(bytes);
            
            // We get to save several bytes per record by storing the int representation of the enum in the parition key
            // Store the name part as upper case so that it will be case insensitive
            // We only want one entry for Azure, azure and AZURE
            return (int)categoryGroup + "|" + encodedName;
        }

        public static string BuildRowKey(Guid profileId)
        {
            return profileId.ToString();
        }

        protected override string BuildPartitionKey()
        {
            return BuildPartitionKey(Value.CategoryGroup, Value.CategoryName);
        }

        protected override string BuildRowKey()
        {
            return BuildRowKey(Value.ProfileId);
        }

        protected override void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            var parts = PartitionKey.Split('|');
            var group = parts[0];
            var encodedName = parts[1];
            var nameBytes = Convert.FromBase64String(encodedName);
            var name = Encoding.UTF8.GetString(nameBytes);

            Value.CategoryGroup = (CategoryGroup)Enum.Parse(typeof(CategoryGroup), group);
            Value.CategoryName = name;
            Value.ProfileId = Guid.Parse(RowKey);

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            properties.Remove(nameof(CategoryLink.CategoryGroup));
            properties.Remove(nameof(CategoryLink.CategoryName));
            properties.Remove(nameof(CategoryLink.ProfileId));

            base.WriteValues(properties, operationContext);
        }
    }
}