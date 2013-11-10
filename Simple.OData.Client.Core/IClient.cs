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
        T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters) where T : class, new();
        T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters) where T : class, new();
    
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
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters) where T : class, new();
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters) where T : class, new();
    }

    public interface IClient<T> : IClient 
        where T : class, new()
    {
        new IEnumerable<T> FindEntries();
        new IEnumerable<T> FindEntries(bool scalarResult);
        new IEnumerable<T> FindEntries(out int totalCount);
        new IEnumerable<T> FindEntries(bool scalarResult, out int totalCount);
        new T FindEntry();
        new T InsertEntry(bool resultRequired = true);
        new void LinkEntry<U>(U linkedEntryKey, string linkName = null);
        new void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        new void UnlinkEntry<U>(string linkName = null);
        new void UnlinkEntry<U>(Expression<Func<T, U>> expression);
        new IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters);

        new Task<IEnumerable<T>> FindEntriesAsync();
        new Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult);
        new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync();
        new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult);
        new Task<T> FindEntryAsync();
        new Task<T> InsertEntryAsync(bool resultRequired = true);
        new Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null);
        new Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        new Task UnlinkEntryAsync<U>(string linkName = null);
        new Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression);
        new Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
    }
}