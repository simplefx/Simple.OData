using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IFluentClient<T>
        where T : class
    {
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<T> FindEntries();
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<T> FindEntries(bool scalarResult);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<T> FindEntries(out int totalCount);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<T> FindEntries(bool scalarResult, out int totalCount);
        [Obsolete("Use asynchronous method instead version", false)]
        T FindEntry();
        [Obsolete("Use asynchronous method instead version", false)]
        object FindScalar();
        [Obsolete("Use asynchronous method instead version", false)]
        T InsertEntry(bool resultRequired = true);
        [Obsolete("Use asynchronous method instead version", false)]
        int UpdateEntry();
        [Obsolete("Use asynchronous method instead version", false)]
        int UpdateEntries();
        [Obsolete("Use asynchronous method instead version", false)]
        int DeleteEntry();
        [Obsolete("Use asynchronous method instead version", false)]
        int DeleteEntries();
        [Obsolete("Use asynchronous method instead version", false)]
        void LinkEntry<U>(U linkedEntryKey, string linkName = null);
        [Obsolete("Use asynchronous method instead version", false)]
        void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey);
        [Obsolete("Use asynchronous method instead version", false)]
        void UnlinkEntry<U>(string linkName = null);
        [Obsolete("Use asynchronous method instead version", false)]
        void UnlinkEntry<U>(Expression<Func<T, U>> expression);
        [Obsolete("Use asynchronous method instead version", false)]
        void UnlinkEntry(ODataExpression expression);
        [Obsolete("Use asynchronous method instead version", false)]
        IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
        [Obsolete("Use asynchronous method instead version", false)]
        T ExecuteFunctionAsScalar(string functionName, IDictionary<string, object> parameters);
        [Obsolete("Use asynchronous method instead version", false)]
        T[] ExecuteFunctionAsArray(string functionName, IDictionary<string, object> parameters);
        [Obsolete("Use asynchronous method instead version", false)]
        string GetCommandText();

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
        Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters);
        Task<string> GetCommandTextAsync();

        IFluentClient<IDictionary<string, object>> As(string derivedCollectionName);
        IFluentClient<U> As<U>(string derivedCollectionName = null) where U : class;
        IFluentClient<ODataEntry> As(ODataExpression expression);
        IFluentClient<T> Key(params object[] entryKey);
        IFluentClient<T> Key(IEnumerable<object> entryKey);
        IFluentClient<T> Key(IDictionary<string, object> entryKey);
        IFluentClient<T> Key(T entryKey);
        IFluentClient<T> Filter(string filter);
        IFluentClient<T> Filter(ODataExpression expression);
        IFluentClient<T> Filter(Expression<Func<T, bool>> expression);
        IFluentClient<T> Skip(int count);
        IFluentClient<T> Top(int count);
        IFluentClient<T> Expand(IEnumerable<string> associations);
        IFluentClient<T> Expand(params string[] associations);
        IFluentClient<T> Expand(params ODataExpression[] associations);
        IFluentClient<T> Expand(Expression<Func<T, object>> expression);
        IFluentClient<T> Select(IEnumerable<string> columns);
        IFluentClient<T> Select(params string[] columns);
        IFluentClient<T> Select(params ODataExpression[] columns);
        IFluentClient<T> Select(Expression<Func<T, object>> expression);
        IFluentClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        IFluentClient<T> OrderBy(params string[] columns);
        IFluentClient<T> OrderBy(params ODataExpression[] columns);
        IFluentClient<T> OrderBy(Expression<Func<T, object>> expression);
        IFluentClient<T> ThenBy(Expression<Func<T, object>> expression);
        IFluentClient<T> OrderByDescending(params string[] columns);
        IFluentClient<T> OrderByDescending(params ODataExpression[] columns);
        IFluentClient<T> OrderByDescending(Expression<Func<T, object>> expression);
        IFluentClient<T> ThenByDescending(Expression<Func<T, object>> expression);
        IFluentClient<T> Count();
        IFluentClient<U> NavigateTo<U>(string linkName = null) where U : class;
        IFluentClient<U> NavigateTo<U>(Expression<Func<T, U>> expression) where U : class;
        IFluentClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class;
        IFluentClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class;
        IFluentClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class;
        IFluentClient<IDictionary<string, object>> NavigateTo(string linkName);
        IFluentClient<T> NavigateTo(ODataExpression expression);
        IFluentClient<T> Set(object value);
        IFluentClient<T> Set(IDictionary<string, object> value);
        IFluentClient<T> Set(T entry);
        IFluentClient<T> Set(params ODataExpression[] value);
        IFluentClient<T> Function(string functionName);
        IFluentClient<T> Parameters(IDictionary<string, object> parameters);

        bool FilterIsKey { get; }
        IDictionary<string, object> FilterAsKey { get; }
    }
}
