using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClient<T>
        where T : class
    {
        IEnumerable<T> FindEntries();
        IEnumerable<T> FindEntries(bool scalarResult);
        IEnumerable<T> FindEntries(out int totalCount);
        IEnumerable<T> FindEntries(bool scalarResult, out int totalCount);
        T FindEntry();
        object FindScalar();
        T InsertEntry(bool resultRequired = true);
        int UpdateEntry();
        int UpdateEntries();
        int DeleteEntry();
        int DeleteEntries();
        void LinkEntry<U>(U linkedEntryKey, string linkName = null);
        void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey);
        void UnlinkEntry<U>(string linkName = null);
        void UnlinkEntry<U>(Expression<Func<T, U>> expression);
        void UnlinkEntry(ODataExpression expression);
        IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
        T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters);
        T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters);

        Task<IEnumerable<T>> FindEntriesAsync();
        Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult);
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync();
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult);
        Task<T> FindEntryAsync();
        Task<object> FindScalarAsync();
        Task<T> InsertEntryAsync(bool resultRequired = true);
        Task<int> UpdateEntryAsync();
        Task<int> DeleteEntryAsync();
        Task<int> DeleteEntriesAsync();
        Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null);
        Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey);
        Task UnlinkEntryAsync<U>(string linkName = null);
        Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression);
        Task UnlinkEntryAsync(ODataExpression expression);
        Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters);
    }
}