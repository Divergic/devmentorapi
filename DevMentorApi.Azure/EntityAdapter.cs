namespace DevMentorApi.Azure
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public abstract class EntityAdapter<T> : ITableEntity where T : class, new()
    {
        private static readonly object _syncLock = new object();
        private static List<AdditionalPropertyMetadata> _additionalProperties;
        private string _partitionKey;
        private string _rowKey;

        protected EntityAdapter()
        {
        }

        protected EntityAdapter(T value)
        {
            Ensure.That(value, nameof(value)).IsNotNull();

            Value = value;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0",
            Justification = "Parameter is validated using CodeGuard.")]
        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            Ensure.That(properties, nameof(properties)).IsNotNull();

            Value = new T();

            TableEntity.ReadUserObject(Value, properties, operationContext);

            var additionalMappings = GetAdditionPropertyMappings(Value, operationContext);

            if (additionalMappings.Count > 0)
            {
                ReadAdditionalProperties(properties, additionalMappings);
            }

            ReadValues(properties, operationContext);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = TableEntity.WriteUserObject(Value, operationContext);

            var additionalMappings = GetAdditionPropertyMappings(Value, operationContext);

            if (additionalMappings.Count > 0)
            {
                WriteAdditionalProperties(additionalMappings, properties);
            }

            WriteValues(properties, operationContext);

            return properties;
        }

        protected abstract string BuildPartitionKey();

        protected abstract string BuildRowKey();

        protected void ClearCache()
        {
            lock (_syncLock)
            {
                _additionalProperties = null;
            }
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0",
            Justification = "Parameter is validated using CodeGuard.")]
        protected virtual TypeConverter GetTypeConverter(PropertyInfo propertyInfo)
        {
            Ensure.That(propertyInfo, nameof(propertyInfo)).IsNotNull();

            var propertyType = propertyInfo.PropertyType;

            return TypeDescriptor.GetConverter(propertyType);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0",
            Justification = "The parameter is validated using CodeGuard.")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "The parameter is validated using CodeGuard.")]
        protected virtual void ReadAdditionalProperty(PropertyInfo propertyInfo, EntityProperty propertyValue)
        {
            Ensure.That(propertyInfo, nameof(propertyInfo)).IsNotNull();
            Ensure.That(propertyValue, nameof(propertyValue)).IsNotNull();

            var converter = GetTypeConverter(propertyInfo);

            try
            {
                var convertedValue = converter.ConvertFromInvariantString(propertyValue.StringValue);

                propertyInfo.SetValue(Value, convertedValue);
            }
            catch (NotSupportedException ex)
            {
                const string MessageFormat = "Failed to write string value to the '{0}' property.";
                var message = string.Format(CultureInfo.CurrentCulture, MessageFormat, propertyInfo.Name);

                throw new NotSupportedException(message, ex);
            }
        }

        protected virtual void ReadValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0",
            Justification = "The parameter is validated using CodeGuard.")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "The parameter is validated using CodeGuard.")]
        protected virtual void WriteAdditionalProperty(
            IDictionary<string, EntityProperty> properties,
            PropertyInfo propertyInfo,
            object propertyValue)
        {
            Ensure.That(properties, nameof(properties)).IsNotNull();
            Ensure.That(propertyInfo, nameof(propertyInfo)).IsNotNull();

            var converter = GetTypeConverter(propertyInfo);

            try
            {
                var convertedValue = converter.ConvertToInvariantString(propertyValue);

                properties[propertyInfo.Name] = EntityProperty.GeneratePropertyForString(convertedValue);
            }
            catch (NotSupportedException ex)
            {
                const string MessageFormat = "Failed to convert property '{0}' to a string value.";
                var message = string.Format(CultureInfo.CurrentCulture, MessageFormat, propertyInfo.Name);

                throw new NotSupportedException(message, ex);
            }
        }

        protected virtual void WriteValues(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
        }

        private static List<AdditionalPropertyMetadata> GetAdditionPropertyMappings(
            T value,
            OperationContext operationContext)
        {
            if (_additionalProperties != null)
            {
                return _additionalProperties;
            }

            List<AdditionalPropertyMetadata> additionalProperties;

            lock (_syncLock)
            {
                // Check the mappings again to protect against race conditions on the lock
                if (_additionalProperties != null)
                {
                    return _additionalProperties;
                }

                additionalProperties = ResolvePropertyMappings(value, operationContext);

                _additionalProperties = additionalProperties;
            }

            return additionalProperties;
        }

        private static List<AdditionalPropertyMetadata> ResolvePropertyMappings(
            T value,
            OperationContext operationContext)
        {
            var storageSupportedProperties = TableEntity.WriteUserObject(value, operationContext);
            var objectProperties = value.GetType().GetTypeInfo().GetProperties();
            var infrastructureProperties = typeof(ITableEntity).GetTypeInfo().GetProperties();
            var missingProperties = objectProperties.Where(
                objectProperty => storageSupportedProperties.ContainsKey(objectProperty.Name) == false);

            var additionalProperties = missingProperties.Select(
                x => new AdditionalPropertyMetadata
                {
                    IsInfrastructureProperty = infrastructureProperties.Any(y => x.Name == y.Name),
                    PropertyMetadata = x
                });

            return additionalProperties.ToList();
        }

        private void ReadAdditionalProperties(
            IDictionary<string, EntityProperty> properties,
            IEnumerable<AdditionalPropertyMetadata> additionalMappings)
        {
            // Populate the properties missing from ReadUserObject
            foreach (var additionalMapping in additionalMappings)
            {
                var propertyType = additionalMapping.PropertyMetadata.PropertyType;

                if (additionalMapping.IsInfrastructureProperty)
                {
                    ReadInfrastructureProperty(additionalMapping, propertyType);
                }
                else if (properties.ContainsKey(additionalMapping.PropertyMetadata.Name))
                {
                    // This is a property that has an unsupport type
                    // Use a converter to resolve and apply the correct value
                    var propertyValue = properties[additionalMapping.PropertyMetadata.Name];

                    ReadAdditionalProperty(additionalMapping.PropertyMetadata, propertyValue);
                }

                // The else case here is that the model now contains a property that was not originally stored when the entity was last written
                // This property will assume the default value for its type
            }
        }

        private void ReadInfrastructureProperty(AdditionalPropertyMetadata additionalMapping, Type propertyType)
        {
            // We don't want to use a string conversion here
            // Explicitly map the types across
            if (additionalMapping.PropertyMetadata.Name == "Timestamp" &&
                propertyType == typeof(DateTimeOffset))
            {
                additionalMapping.PropertyMetadata.SetValue(Value, Timestamp);
            }
            else if (additionalMapping.PropertyMetadata.Name == "ETag" &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(Value, ETag);
            }
            else if (additionalMapping.PropertyMetadata.Name == "PartitionKey" &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(Value, PartitionKey);
            }
            else if (additionalMapping.PropertyMetadata.Name == "RowKey" &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(Value, RowKey);
            }
            else
            {
                const string UnsupportedPropertyMessage =
                    "The {0} interface now defines a property {1} which is not supported by this adapter.";

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    UnsupportedPropertyMessage,
                    typeof(ITableEntity).FullName,
                    additionalMapping.PropertyMetadata.Name);

                throw new InvalidOperationException(message);
            }
        }

        private void WriteAdditionalProperties(
            IEnumerable<AdditionalPropertyMetadata> additionalMappings,
            IDictionary<string, EntityProperty> properties)
        {
            // Populate the properties missing from WriteUserObject
            foreach (var additionalMapping in additionalMappings)
            {
                if (additionalMapping.IsInfrastructureProperty)
                {
                    // We need to let the storage mechanism handle the write of the infrastructure properties
                    continue;
                }

                var propertyValue = additionalMapping.PropertyMetadata.GetValue(Value);

                WriteAdditionalProperty(properties, additionalMapping.PropertyMetadata, propertyValue);
            }
        }

        public string ETag { get; set; }

        public string PartitionKey
        {
            get
            {
                if (_partitionKey == null)
                {
                    _partitionKey = BuildPartitionKey();
                }

                return _partitionKey;
            }
            set => _partitionKey = value;
        }

        public string RowKey
        {
            get
            {
                if (_rowKey == null)
                {
                    _rowKey = BuildRowKey();
                }

                return _rowKey;
            }
            set => _rowKey = value;
        }

        public DateTimeOffset Timestamp { get; set; }

        public T Value { get; private set; }

        private struct AdditionalPropertyMetadata
        {
            public bool IsInfrastructureProperty { get; set; }

            public PropertyInfo PropertyMetadata { get; set; }
        }
    }
}