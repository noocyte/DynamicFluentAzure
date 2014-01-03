using System;
using UXRisk.Lib.Common.Models;

namespace DynamicFluentAzure.Tests.Helpers
{
    public static class JsonObjectFactory
    {
        public static JsonObject[] CreateJsonObjectsForBatch(int numberOfObjects = 2)
        {
            var json = new JsonObject[numberOfObjects];

            for (var i = 1; i <= numberOfObjects; i++)
                json[i - 1] = CreateJsonObjectForPost(i.ToString());

            return json;
        }

        public static JsonObject CreateJsonObjectForPostWithArray(string id = "someId", string parentId = "",
            string name = "someName")
        {
            var json = new JsonObject
            {
                {"id", id},
                {"name", name},
                {"parentId", parentId},
                {"PartitionKey", "PK"},
                {"RowKey", id},
                {"dragon_ids", new object[] {"1", "2", "3"}}
            };
            json.EnsureValidSystemProperties();
            return json;
        }

        public static JsonObject CreateJsonObjectForPost(string id = "someId", string parentId = "",
            string name = "someName")
        {
            var json = new JsonObject
            {
                {"id", id},
                {"name", name},
                {"parentId", parentId},
                {"PartitionKey", "PK"},
                {"dragon_ids", new object[] {"1", "2", "3"}},
                {"RowKey", id}
            };
            json.EnsureValidSystemProperties();
            return json;
        }

        public static JsonObject CreateJsonObject(DateTime aTimestamp, string id = "something")
        {
            const string valueString = "something";
            var json = new JsonObject
            {
                {"ETag", valueString},
                {"Timestamp", aTimestamp},
                {"id", id},
                {"name", valueString}
            };
            json.EnsureValidSystemProperties();
            return json;
        }

        public static JsonObject CreateJsonObjectWithTimestamp(DateTimeOffset aTimestamp, string id = "something")
        {
            const string valueString = "something";
            var json = new JsonObject
            {
                {"ETag", valueString},
                {"Timestamp", aTimestamp},
                {"id", id},
                {"name", valueString}
            };
            json.EnsureValidSystemProperties();
            return json;
        }

        public static JsonObject CreateJsonObjectWithETag()
        {
            const string valueString = "something";
            var json = new JsonObject {{"ETag", valueString}, {"id", valueString}, {"name", valueString}};
            json.EnsureValidSystemProperties();
            return json;
        }
    }
}