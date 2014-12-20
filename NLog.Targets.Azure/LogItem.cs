using System;
using System.Collections.Generic;
using AzureStorageExtensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NLog.Targets
{
    public class LogItem : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        public DateTime LogDateTime { get; set; }

        readonly Dictionary<string, string> _properties = new Dictionary<string, string>(); 
        public string this[string name]
        {
            get
            {
                string value;
                if (_properties.TryGetValue(name, out value))
                    return value;
                else
                    return null;
            }
            set { _properties[name] = value; }
        }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            ExpandableTableEntity.ShrinkDictionary(properties);
            foreach (var entityProperty in properties)
            {
                if (entityProperty.Key == "LogDateTime")
                {
                    this.LogDateTime = entityProperty.Value.DateTime.GetValueOrDefault();
                    continue;
                }
                if (entityProperty.Value.PropertyType != EdmType.String)
                    continue;
                switch (entityProperty.Key)
                {
                    case "PartitionKey":
                    case "RowKey":
                    case "ETag":
                        continue;
                }
                _properties.Add(entityProperty.Key, entityProperty.Value.StringValue);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var dict = new Dictionary<string, EntityProperty>
            {
                {"LogDateTime", new EntityProperty(this.LogDateTime)}
            };
            foreach (var property in _properties)
                dict.Add(property.Key, new EntityProperty(property.Value));
            ExpandableTableEntity.ExpandDictionary(dict);
            return dict;
        }
    }
}
