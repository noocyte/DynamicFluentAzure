using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UXRisk.Lib.Common.Models;
using Microsoft.WindowsAzure.Storage.Table;
using UXRisk.Lib.Common.Infrastructure;
using System.Globalization;

namespace DynamicFluentAzure
{
    public static class JsonObjectMapper
    {
        public static JsonObject ToJsonObject(this DynamicTableEntity ce)
        {
            var json = new JsonObject { { "ETag", ce.ETag }, { "Timestamp", ce.Timestamp } };
            foreach (var field in ce.Properties)
            {
                json[field.Key] =
                    IsArray(field)
                        ? JsonConvert.DeserializeObject<object[]>(field.Value.StringValue)
                        : field.Value.PropertyAsObject;
            }
            json.EnsureValidSystemProperties();
            return json;
        }

        private static bool IsArray(KeyValuePair<string, EntityProperty> field)
        {
            return field.Value.PropertyType.Equals("String") &&
                   field.Key.Contains("_ids") &&
                   field.Value.StringValue.StartsWith("[") &&
                   field.Value.StringValue.EndsWith("]");
        }

        public static DynamicTableEntity ToDynamicEntity(this JsonObject oneObject)
        {
            if (oneObject == null)
                throw new ArgumentNullException("oneObject");
            var dict = new Dictionary<string, EntityProperty>();
            foreach (var item in oneObject)
            {
                if (item.Key.Equals(EntityConstants.PartitionKey.ToLowerInvariant()) ||
                    item.Key.Equals(EntityConstants.RowKey.ToLowerInvariant()) ||
                     item.Key.Equals("etag"))
                    continue;

                EntityProperty property = null;
                var type = item.Value.GetType();
                var value = item.Value;

                if (type == typeof(string))
                    property = new EntityProperty((string)value);
                else if (type == typeof(int))
                    property = new EntityProperty((int)value);
                else if (type == typeof(long))
                    property = new EntityProperty((long)value);
                else if (type == typeof(double))
                    property = new EntityProperty((double)value);
                else if (type == typeof(Guid))
                    property = new EntityProperty((Guid)value);
                else if (type == typeof(bool))
                    property = new EntityProperty((bool)value);
                else if (type.IsEnum)
                {
                    var typeCode = ((Enum)value).GetTypeCode();
                    if (typeCode <= TypeCode.Int32)
                        property = new EntityProperty(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                    else
                        property = new EntityProperty(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                }
                else if (type == typeof(byte[]))
                    property = new EntityProperty((byte[])value);

                if (property != null)
                    dict.Add(item.Key, property);
            }

            var etag = oneObject.ContainsKey("etag") ? oneObject["etag"].ToString() : null;
            return new DynamicTableEntity("PK", oneObject.Id, etag, dict);
        }
    }
}