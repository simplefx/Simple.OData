using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IODataClient
    {
        IFluentClient<IDictionary<string, object>> For(string collectionName);
        IFluentClient<ODataEntry> For(ODataExpression expression);
        IFluentClient<T> For<T>(string collectionName = null) where T : class;

        Task<ISchema> GetSchemaAsync();
        Task<ISchema> GetSchemaAsync(CancellationToken cancellationToken);

        Task<string> GetSchemaAsStringAsync();
        Task<string> GetSchemaAsStringAsync(CancellationToken cancellationToken);
        
        Task<string> GetCommandTextAsync(string collection, ODataExpression expression);
        Task<string> GetCommandTextAsync(string collection, ODataExpression expression, CancellationToken cancellationToken);
        Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression);
        Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression, CancellationToken cancellationToken);
        
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText);
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, CancellationToken cancellationToken);
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult);
        Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult, CancellationToken cancellationToken);
        
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, CancellationToken cancellationToken);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult);
        Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult, CancellationToken cancellationToken);
        
        Task<IDictionary<string, object>> FindEntryAsync(string commandText);
        Task<IDictionary<string, object>> FindEntryAsync(string commandText, CancellationToken cancellationToken);
        
        Task<object> FindScalarAsync(string commandText);
        Task<object> FindScalarAsync(string commandText, CancellationToken cancellationToken);
        
        Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey);
        Task<IDictionary<string, object>> GetEntryAsync(string collection, CancellationToken cancellationToken, params object[] entryKey);
        Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey);
        Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken);
        
        Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData);
        Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, CancellationToken cancellationToken);
        Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired);
        Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken);
        
        Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, CancellationToken cancellationToken);
        Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired);
        Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken);
        
        Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData);
        Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, CancellationToken cancellationToken);
        Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired);
        Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken);
        
        Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey);
        Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken);
        Task<int> DeleteEntriesAsync(string collection, string commandText);
        Task<int> DeleteEntriesAsync(string collection, string commandText, CancellationToken cancellationToken);
        
        Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken);
        
        Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName);
        Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken);
        
        Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
        
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
        
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters);
        Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);
    }
}
