using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IODataClient
    {
        ISchema Schema { get; }
        string SchemaAsString { get; }
        Task<string> SchemaAsStringAsync { get; }

        IFluentClient<IDictionary<string, object>> For(string collectionName);
        IFluentClient<ODataEntry> For(ODataExpression expression);
        IFluentClient<T> For<T>(string collectionName = null) where T : class;

        IEnumerable<IDictionary<string, object>> FindEntries(string commandText);
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult);
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount);
        IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, out int totalCount);
        IDictionary<string, object> FindEntry(string commandText);
        object FindScalar(string commandText);
        IDictionary<string, object> GetEntry(string collection, params object[] entryKey);
        IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey);
        IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true);
        int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData);
        int UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        int DeleteEntries(string collection, string commandText);
        int DeleteEntry(string collection, IDictionary<string, object> entryKey);
        void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName);
        IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
        T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters);
        T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters);

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
        Task<int> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        Task<int> DeleteEntriesAsync(string collection, string commandText);
        Task<int> DeleteEntryAsync(string collection, IDictionary<string, object> entryKey);
        Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName);
        Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters);

        string FormatCommand(string collection, ODataExpression expression);
        string FormatCommand<T>(string collection, Expression<Func<T, bool>> expression);
    }
}
