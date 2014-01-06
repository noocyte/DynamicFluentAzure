using System.Configuration;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using UXRisk.Lib.Common.Interfaces.Services;
using UXRisk.Lib.Common.Models;
using UXRisk.Lib.Common.Services;

namespace DynamicFluentAzure.Tests.Helpers
{
    public static class FluentAzureTestsHelper
    {
        internal static void AddAzureSpecificStuff(Response<JsonObject> updatedJson, string entityId)
        {
            updatedJson.Result.Add("RowKey", entityId);
            updatedJson.Result.Add("PartitionKey", "PK");
        }


        internal static CloudStorageAccount GetAccount()
        {
            return CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnection"]);
        }

        internal static IAzureTable<T> GetAzureTable<T>() where T : ITableEntity, new()
        {
            return new AzureTable<T>(GetAccount());
        }

        internal static async Task<Response<JsonObject>> GivenOldETag(FluentAzure client, string tableName)
        {
            const string entityId = "one";

            var firstEntity = JsonObjectFactory.CreateJsonObjectForPost(entityId);
            var firstResponse = await client.IntoTable(tableName).PostAsync(firstEntity).ConfigureAwait(false);

            var secondEntity = await client.FromTable(tableName).GetByIdAsync(entityId).ConfigureAwait(false);
            secondEntity.Result.Add("newField", "newValue");
            AddAzureSpecificStuff(secondEntity, entityId);

            await client.IntoTable(tableName).MergeAsync(secondEntity.Result).ConfigureAwait(false);
            return firstResponse;
        }

        internal static CloudTableClient GetTableClient()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnection"));
            var client = storageAccount.CreateCloudTableClient();
            client.PayloadFormat = TablePayloadFormat.Json;
            return client;
        }
    }
}