namespace TechMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TechMentorApi.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class CategoryAdapter : EntityAdapter<Category>
    {
        public CategoryAdapter()
        {
        }

        public CategoryAdapter(Category category) : base(category)
        {
        }

        public static string BuildPartitionKey(CategoryGroup group)
        {
            // We get to save several bytes per record by storing the int representation of the enum in the parition key
            return ((int)group).ToString();
        }

        public static string BuildRowKey(string name)
        {
            // Store the name as the row key so that it will be case insensitive
            // We only want one entry for Azure, azure and AZURE
            var invariantName = name.ToUpperInvariant();
            var bytes = Encoding.UTF8.GetBytes(invariantName);

            var key = Convert.ToBase64String(bytes);

            return key;
        }

        protected override string BuildPartitionKey()
        {
            return BuildPartitionKey(Value.Group);
        }

        protected override string BuildRowKey()
        {
            return BuildRowKey(Value.Name);
        }

        protected override void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            Value.Group = (CategoryGroup)Enum.Parse(typeof(CategoryGroup), PartitionKey);

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            // We can't remove the rowkey because we won't know the original casing once the record is read back out again because 
            // BuildRowKey uses ToUpperInvariant to avoid duplicates of the same logical value
            properties.Remove(nameof(Category.Group));

            base.WriteValues(properties, operationContext);
        }
    }
}