using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClient<T> : IClient
        where T : class
    {
        new IEnumerable<T> FindEntries();
        new IEnumerable<T> FindEntries(bool scalarResult);
        new IEnumerable<T> FindEntries(out int totalCount);
        new IEnumerable<T> FindEntries(bool scalarResult, out int totalCount);
        new T FindEntry();
        new T InsertEntry(bool resultRequired = true);
        new void LinkEntry<U>(U linkedEntryKey, string linkName = null);
        new void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        new void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey);
        new void UnlinkEntry<U>(string linkName = null);
        new void UnlinkEntry<U>(Expression<Func<T, U>> expression);
        new void UnlinkEntry(ODataExpression expression);
        new IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters);

        new Task<IEnumerable<T>> FindEntriesAsync();
        new Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult);
        new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync();
        new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult);
        new Task<T> FindEntryAsync();
        new Task<T> InsertEntryAsync(bool resultRequired = true);
        new Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null);
        new Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        new Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey);
        new Task UnlinkEntryAsync<U>(string linkName = null);
        new Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression);
        new Task UnlinkEntryAsync(ODataExpression expression);
        new Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
    }
}