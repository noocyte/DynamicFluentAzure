using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UXRisk.Lib.Common.Models;

namespace DynamicFluentAzure
{
    public class FluentCyan : IFluentCyan
    {
        private string _tableName;
        private CloudTableClient _client;
        private static IDictionary<string, CloudTable> _tables;

        public FluentCyan(CloudTableClient client)
        {
            _client = client;
            _tables = new Dictionary<string, CloudTable>();
        }

        public IFluentCyan IntoTable(string tableName)
        {
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            _tableName = tableName;
            return this;
        }

        public IFluentCyan FromTable(string tableName)
        {
            if (String.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            _tableName = tableName;
            return this;
        }

        public async Task<Response<JsonObject>> PostAsync(JsonObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var table = await DefineTable().ConfigureAwait(false);
            var entity = json.ToDynamicEntity();
            TableOperation insertOperation = TableOperation.Insert(entity);
            var result = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            entity = result.Result as DynamicTableEntity;

            return new Response<JsonObject>(HttpStatusCode.Created, entity.ToJsonObject());
        }

        public async Task<Response<JsonObject>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            var table = await DefineTable().ConfigureAwait(false);

            var query =
                new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.CombineFilters(
                 TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "PK"),
                 TableOperators.And,
                 TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)));
            query.Where(TableQuery.GenerateFilterConditionForBool("sys_deleted", "ne", true));


            var items = table.ExecuteQuery(query);
                //.Query("PK", id, filter: "sys_deleted ne true").ConfigureAwait(false);
            var result = items.ToList();
            var json = new JsonObject();
            var status = HttpStatusCode.NotFound;

            // ReSharper disable once UseMethodAny.0
            if (result.Count() > 0)
            {
                json = result.First().ToJsonObject();
                status = HttpStatusCode.OK;
            }

            return new Response<JsonObject>(status, json);
        }

        public async Task<Response<IEnumerable<JsonObject>>> GetAllAsync()
        {
            var table = await DefineTable().ConfigureAwait(false);

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

            var table = await DefineTable().ConfigureAwait(false);
            var entity = json.ToDynamicEntity();

            var operation = TableOperation.Merge(entity);
            var result = table.Execute(operation);
            entity = result.Result as DynamicTableEntity;

            return new Response<JsonObject>(HttpStatusCode.OK, entity.ToJsonObject());
        }

        public async Task<Response<JsonObject>> DeleteAsync(JsonObject json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            var table = await DefineTable().ConfigureAwait(false);
            var entity = json.ToDynamicEntity();

            var operation = TableOperation.Delete(entity);
            var result = table.Execute(operation);
            entity = result.Result as DynamicTableEntity;

            return new Response<JsonObject>(HttpStatusCode.OK, entity.ToJsonObject());
        }

        internal async Task<CloudTable> DefineTable()
        {
            _tableName = _tableName.ToLowerInvariant();
            if (_tables.ContainsKey(_tableName))
                return _tables[_tableName];

            var table = _client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            _tables.Add(_tableName, table);
            return _tables[_tableName];
        }
    }
}