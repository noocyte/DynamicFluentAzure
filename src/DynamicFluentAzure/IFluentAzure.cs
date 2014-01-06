using System.Collections.Generic;
using System.Threading.Tasks;
using UXRisk.Lib.Common.Models;

namespace DynamicFluentAzure
{
    public interface IFluentAzure
    {
        IFluentAzure IntoTable(string tableName);
        IFluentAzure FromTable(string tableName);
        Task<Response<JsonObject>> PostAsync(JsonObject json);
        Task<IEnumerable<Response<JsonObject>>> BatchPostAsync(IEnumerable<JsonObject> jsonObjects);
        Task<Response<JsonObject>> GetByIdAsync(string id);
        Task<Response<IEnumerable<JsonObject>>> GetAllAsync();
        Task<Response<JsonObject>> MergeAsync(JsonObject json);
        Task<Response<JsonObject>> DeleteAsync(JsonObject json);
    }
}