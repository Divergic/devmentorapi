namespace DevMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using DevMentorApi.Model;
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
            return group.ToString();
        }

        public static string BuildRowKey(string name)
        {
            return name;
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
            Value.Name = RowKey;

            base.ReadValues(properties, operationContext);
        }

        protected override void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            properties.Remove(nameof(Category.Group));
            properties.Remove(nameof(Category.Name));

            base.WriteValues(properties, operationContext);
        }
    }
}