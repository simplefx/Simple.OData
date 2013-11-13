using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClient
    {
        IEnumerable<IDictionary<string, object>> FindEntries();
        IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult);
        IEnumerable<IDictionary<string, object>> FindEntries(out int totalCount);
        IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, out int totalCount);
        IDictionary<string, object> FindEntry();
        object FindScalar();
        IDictionary<string, object> InsertEntry(bool resultRequired = true);
        int UpdateEntry();
        int UpdateEntries();
        int DeleteEntry();
        int DeleteEntries();
        void LinkEntry(string linkName, IDictionary<string, object> linkedEntryKey);
        void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        void UnlinkEntry(string linkName);
        void UnlinkEntry(ODataExpression expression);
        IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
        T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters);
        T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters);
    
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync();
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(bool scalarResult);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync();
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(bool scalarResult);
        Task<IDictionary<string, object>> FindEntryAsync();
        Task<object> FindScalarAsync();
        Task<IDictionary<string, object>> InsertEntryAsync(bool resultRequired = true);
        Task<int> UpdateEntryAsync();
        Task<int> DeleteEntryAsync();
        Task<int> DeleteEntriesAsync();
        Task LinkEntryAsync(string linkName, IDictionary<string, object> linkedEntryKey);
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        Task UnlinkEntryAsync(string linkName);
        Task UnlinkEntryAsync(ODataExpression expression);
        Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters);
    }
}