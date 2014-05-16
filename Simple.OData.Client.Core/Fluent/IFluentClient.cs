using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IFluentClient<T>
        where T : class
    {
        Task<IEnumerable<T>> FindEntriesAsync();
        Task<IEnumerable<T>> FindEntriesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult);
        Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult, CancellationToken cancellationToken);

        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync();
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(CancellationToken cancellationToken);
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult);
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult, CancellationToken cancellationToken);

        Task<T> FindEntryAsync();
        Task<T> FindEntryAsync(CancellationToken cancellationToken);

        Task<object> FindScalarAsync();
        Task<object> FindScalarAsync(CancellationToken cancellationToken);

        Task<T> InsertEntryAsync();
        Task<T> InsertEntryAsync(CancellationToken cancellationToken);
        Task<T> InsertEntryAsync(bool resultRequired);
        Task<T> InsertEntryAsync(bool resultRequired, CancellationToken cancellationToken);

        Task<T> UpdateEntryAsync();
        Task<T> UpdateEntryAsync(CancellationToken cancellationToken);
        Task<T> UpdateEntryAsync(bool resultRequired);
        Task<T> UpdateEntryAsync(bool resultRequired, CancellationToken cancellationToken);

        Task<IEnumerable<T>> UpdateEntriesAsync();
        Task<IEnumerable<T>> UpdateEntriesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired);
        Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired, CancellationToken cancellationToken);

        Task DeleteEntryAsync();
        Task DeleteEntryAsync(CancellationToken cancellationToken);
        
        Task<int> DeleteEntriesAsync();
        Task<int> DeleteEntriesAsync(CancellationToken cancellationToken);
        
        Task LinkEntryAsync<U>(U linkedEntryKey);
        Task LinkEntryAsync<U>(U linkedEntryKey, CancellationToken cancellationToken);
        Task LinkEntryAsync<U>(U linkedEntryKey, string linkName);
        Task LinkEntryAsync<U>(U linkedEntryKey, string linkName, CancellationToken cancellationToken);
        Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey, CancellationToken cancellationToken);
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken);
        Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey);
        Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey, CancellationToken cancellationToken);
        
        Task UnlinkEntryAsync<U>();
        Task UnlinkEntryAsync<U>(CancellationToken cancellationToken);
        Task UnlinkEntryAsync<U>(string linkName);
        Task UnlinkEntryAsync<U>(string linkName, CancellationToken cancellationToken);
        Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression);
        Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, CancellationToken cancellationToken);
        Task UnlinkEntryAsync(ODataExpression expression);
        Task UnlinkEntryAsync(ODataExpression expression, CancellationToken cancellationToken);
        
        Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
        
        Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
        
        Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
        
        Task<string> GetCommandTextAsync();
        Task<string> GetCommandTextAsync(CancellationToken cancellationToken);

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
