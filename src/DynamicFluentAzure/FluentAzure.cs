using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using UXRisk.Lib.Common.Models;

namespace DynamicFluentAzure
{
    public class FluentAzure : IFluentAzure
    {
        internal static IDictionary<string, CloudTable> Tables;
        private readonly CloudTableClient _client;
        private string _tableName;

        public FluentAzure(CloudTableClient client)
        {
            _client = client;
            Tables = new Dictionary<string, CloudTable>();
        }

        public IFluentAzure IntoTable(string tableName)
        {
            return FromTable(tableName);
        }

        public IFluentAzure FromTable(string tableName)
        {
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            _tableName = tableName;
            return this;
        }

        public async Task<IEnumerable<Response<JsonObject>>> BatchPostAsync(IEnumerable<JsonObject> jsonObjects)
        {
            if (jsonObjects == null)
                throw new ArgumentNullException("jsonObjects");

            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);
            var batch = new TableBatchOperation();

            foreach (var entity in jsonObjects.Select(json => json.ToDynamicEntity()))
                batch.Add(TableOperation.Insert(entity));

            var results = await table.ExecuteBatchAsync(batch).ConfigureAwait(false);

            return results
                .Select(result => result.Result as DynamicTableEntity)
                .Select(entity => entity.ToJsonObject())
                .Select(jsonObj => new Response<JsonObject>(HttpStatusCode.Created, jsonObj));
        }


        public async Task<Response<JsonObject>> PostAsync(JsonObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);
            var entity = json.ToDynamicEntity();
            var insertOperation = TableOperation.Insert(entity);
            var jsonObj = await ExecuteOperationAsync(table, insertOperation).ConfigureAwait(false);

            return new Response<JsonObject>(HttpStatusCode.Created, jsonObj);
        }

        public async Task<Response<JsonObject>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);

            var query =
                new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "PK"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)));
            query.Where(TableQuery.GenerateFilterConditionForBool("sys_deleted", "ne", true));

            var items = table.ExecuteQuery(query);
            var result = items.ToList();
            var json = new JsonObject();
            var status = HttpStatusCode.NotFound;

            // ReSharper disable UseMethodAny.3
            if (result.Count() <= 0) return new Response<JsonObject>(status, json);
            // ReSharper restore UseMethodAny.3
            json = result.First().ToJsonObject();
            status = HttpStatusCode.OK;

            return new Response<JsonObject>(status, json);
        }

        public async Task<Response<IEnumerable<JsonObject>>> GetAllAsync()
        {
            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "PK"));

            var items = table.ExecuteQuery(query);

            var result = items.ToList();

            var status = HttpStatusCode.NotFound;
            var listOfJson = new List<JsonObject>();

            // ReSharper disable once UseMethodAny.0
            if (result.Count() > 0)
            {
                listOfJson.AddRange(result.Select(ce => ce.ToJsonObject()));
                status = HttpStatusCode.OK;
            }

            return new Response<IEnumerable<JsonObject>>(status, listOfJson);
        }

        public async Task<Response<JsonObject>> MergeAsync(JsonObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);
            var entity = json.ToDynamicEntity();

            var operation = TableOperation.Merge(entity);
            var jsonObj = await ExecuteOperationAsync(table, operation).ConfigureAwait(false);

            return new Response<JsonObject>(HttpStatusCode.OK, jsonObj);
        }

        public async Task<Response<JsonObject>> DeleteAsync(JsonObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var table = await DefineTableAsync(CreateCloudTable).ConfigureAwait(false);
            var entity = json.ToDynamicEntity();

            var operation = TableOperation.Delete(entity);
            var jsonObj = await ExecuteOperationAsync(table, operation).ConfigureAwait(false);

            return new Response<JsonObject>(HttpStatusCode.OK, jsonObj);
        }

        internal async Task<CloudTable> DefineTableAsync(Func<Task<CloudTable>> createIfNotExists)
        {
            _tableName = _tableName.ToLowerInvariant();
            if (Tables.ContainsKey(_tableName))
                return Tables[_tableName];

            var table = await createIfNotExists().ConfigureAwait(false);
            Tables.Add(_tableName, table);
            return Tables[_tableName];
        }

        internal async Task<CloudTable> CreateCloudTable()
        {
            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            return table;
        }

        internal static async Task<JsonObject> ExecuteOperationAsync(CloudTable table, TableOperation operation)
        {
            var result = await table.ExecuteAsync(operation).ConfigureAwait(false);
            var entity = result.Result as DynamicTableEntity;
            var jsonObj = entity.ToJsonObject();
            return jsonObj;
        }
    }
}