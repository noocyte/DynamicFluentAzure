using Microsoft.WindowsAzure.Storage.Table;
using System;
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

            return new Response<JsonObject>(HttpStatusCode.Created, (JsonObject)result.Result);
        }

        //public async Task<Response<JsonObject>> GetByIdAsync(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //        throw new ArgumentNullException("id");

        //    var table = await DefineTable().ConfigureAwait(false);
        //    var items = await table.Query("PK", id, filter: "sys_deleted ne true").ConfigureAwait(false);
        //    var result = items.ToList();
        //    var json = new JsonObject();
        //    var status = HttpStatusCode.NotFound;

        //    // ReSharper disable once UseMethodAny.0
        //    if (result.Count() > 0)
        //    {
        //        json = result.First().ToJsonObject();
        //        status = HttpStatusCode.OK;
        //    }

        //    return new Response<JsonObject>(status, json);
        //}

        //public async Task<Response<IEnumerable<JsonObject>>> GetAllAsync()
        //{
        //    var table = await DefineTable().ConfigureAwait(false);
        //    var items = await table.Query("PK").ConfigureAwait(false);
        //    var result = items.ToList();

        //    var status = HttpStatusCode.NotFound;
        //    var listOfJson = new List<JsonObject>();

        //    // ReSharper disable once UseMethodAny.0
        //    if (result.Count() > 0)
        //    {
        //        listOfJson.AddRange(result.Select(ce => ce.ToJsonObject()));
        //        status = HttpStatusCode.OK;
        //    }

        //    return new Response<IEnumerable<JsonObject>>(status, listOfJson);
        //}

        //public async Task<Response<JsonObject>> MergeAsync(JsonObject json)
        //{
        //    if (json == null)
        //        throw new ArgumentNullException("json");

        //    var table = await DefineTable().ConfigureAwait(false);
        //    var entity = json.ToCyanEntity();
        //    var result = await table.Merge(entity).ConfigureAwait(false);

        //    return new Response<JsonObject>(HttpStatusCode.OK, result.ToJsonObject());
        //}

        //public async Task<Response<JsonObject>> DeleteAsync(JsonObject json)
        //{
        //    if (json == null)
        //        throw new ArgumentNullException("json");

        //    var table = await DefineTable().ConfigureAwait(false);
        //    var entity = json.ToCyanEntity();
        //    var result = new JsonObject();

        //    await table.Delete(entity).ConfigureAwait(false);

        //    return new Response<JsonObject>(HttpStatusCode.OK, result);
        //}

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


        public Task<Response<JsonObject>> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<IEnumerable<JsonObject>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Response<JsonObject>> MergeAsync(JsonObject json)
        {
            throw new NotImplementedException();
        }

        public Task<Response<JsonObject>> DeleteAsync(JsonObject json)
        {
            throw new NotImplementedException();
        }
    }
}