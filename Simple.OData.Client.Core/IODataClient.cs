using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IODataClient
    {
        IFluentClient<IDictionary<string, object>> For(string collectionName);
        IFluentClient<ODataEntry> For(ODataExpression expression);
        IFluentClient<T> For<T>(string collectionName = null) where T : class;

        [Obsolete("Use asynchronous method instead version", false)]
        ISchema GetSchema();
        [Obsolete("Use asynchronous method instead version", false)]
        string GetSchemaAsString();
        [Obsolete("Use asynchronous method instead version", false)]
        string GetCommandText(string collection, ODataExpression expression);
        [Obsolete("Use asynchronous method instead version", false)]
        string GetCommandText<T>(string collection, Expression<Func<T, bool>> expression);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, out int totalCount);
        [Obsolete("Use asynchronous method instead version", false)]
        IDictionary<string, object> FindEntry(string commandText);
        [Obsolete("Use asynchronous method instead version", false)]
        object FindScalar(string commandText);
        [Obsolete("Use asynchronous method instead version", false)]
        IDictionary<string, object> GetEntry(string collection, params object[] entryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true);
        [Obsolete("Use asynchronous method instead version", false)]
        int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData);
        [Obsolete("Use asynchronous method instead version", false)]
        void UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        [Obsolete("Use asynchronous method instead version", false)]
        int DeleteEntries(string collection, string commandText);
        [Obsolete("Use asynchronous method instead version", false)]
        void DeleteEntry(string collection, IDictionary<string, object> entryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
        [Obsolete("Use asynchronous method instead version", false)]
        T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters);
        [Obsolete("Use asynchronous method instead version", false)]
        T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters);

        Task<ISchema> GetSchemaAsync();
        Task<string> GetSchemaAsStringAsync();
        Task<string> GetCommandTextAsync(string collection, ODataExpression expression);
        Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression);
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText);
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult);
        Task<IDictionary<string, object>> FindEntryAsync(string commandText);
        Task<object> FindScalarAsync(string commandText);
        Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey);
        Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey);
        Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired = true);
        Task<int> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData);
        Task UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        Task<int> DeleteEntriesAsync(string collection, string commandText);
        Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey);
        Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName);
        Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters);
    }
}
