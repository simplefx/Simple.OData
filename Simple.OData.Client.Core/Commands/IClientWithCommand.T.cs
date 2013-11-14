using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClientWithCommand<T> : IClientWithCommand, IClient<T>, ICommand<T>
        where T : class
    {
        // see http://stackoverflow.com/questions/10531575/passing-a-dynamic-parameter-throws-runtimebinderexception-when-calling-method-fr

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

        new IClientWithCommand<U> As<U>(string derivedCollectionName = null) where U : class, new();
        new IClientWithCommand<T> Key(params object[] entryKey);
        new IClientWithCommand<T> Key(IEnumerable<object> entryKey);
        new IClientWithCommand<T> Key(IDictionary<string, object> entryKey);
        new IClientWithCommand<T> Key(T entryKey);
        new IClientWithCommand<T> Filter(string filter);
        new IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
        new IClientWithCommand<T> Skip(int count);
        new IClientWithCommand<T> Top(int count);
        new IClientWithCommand<T> Expand(IEnumerable<string> associations);
        new IClientWithCommand<T> Expand(params string[] associations);
        new IClientWithCommand<T> Expand(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> Select(IEnumerable<string> columns);
        new IClientWithCommand<T> Select(params string[] columns);
        new IClientWithCommand<T> Select(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        new IClientWithCommand<T> OrderBy(params string[] columns);
        new IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderByDescending(params string[] columns);
        new IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> Count();
        new IClientWithCommand<U> NavigateTo<U>(string linkName = null) where U : class, new();
        new IClientWithCommand<T> Set(object value);
        new IClientWithCommand<T> Set(T entry);
        new IClientWithCommand<T> Function(string functionName);
        new IClientWithCommand<T> Parameters(IDictionary<string, object> parameters);
    }
}