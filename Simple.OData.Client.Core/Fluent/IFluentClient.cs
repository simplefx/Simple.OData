using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    public interface IFluentClient<T>
        where T : class
    {
        /// <summary>
        /// Retrieves entries by executing OData GET request.
        /// </summary>
        /// <returns>Request execution results.</returns>
        Task<IEnumerable<T>> FindEntriesAsync();
        /// <summary>
        /// Retrieves entries by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entries found.</returns>
        Task<IEnumerable<T>> FindEntriesAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves entries by executing OData GET request.
        /// </summary>
        /// <param name="scalarResult">if set to <c>true</c> the result is expected to be of a scalar type.</param>
        /// <returns>Entries found.</returns>
        Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult);
        /// <summary>
        /// Retrieves entries by executing OData GET request.
        /// </summary>
        /// <param name="scalarResult">if set to <c>true</c> the result is expected to be of a scalar type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entries found.</returns>
        Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves entries with result count by executing OData GET request.
        /// </summary>
        /// <returns>Entries found with entry count.</returns>
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync();
        /// <summary>
        /// Retrieves entries with result count by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entries found with entry count.</returns>
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves entries with result count by executing OData GET request.
        /// </summary>
        /// <param name="scalarResult">if set to <c>true</c> the result is expected to be of a scalar type.</param>
        /// <returns>Entries found with entry count.</returns>
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult);
        /// <summary>
        /// Retrieves entries with result count by executing OData GET request.
        /// </summary>
        /// <param name="scalarResult">if set to <c>true</c> the result is expected to be of a scalar type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entries found with entry count.</returns>
        Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves an entry by executing OData GET request.
        /// </summary>
        /// <returns>The first of the entries found.</returns>
        Task<T> FindEntryAsync();
        /// <summary>
        /// Retrieves an entry by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first of the entries found.</returns>
        Task<T> FindEntryAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves an entry as a scalar type by executing OData GET request.
        /// </summary>
        /// <returns>The result as a scalar type.</returns>
        Task<object> FindScalarAsync();
        /// <summary>
        /// Retrieves an entry as a scalar type by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result as a scalar type.</returns>
        Task<object> FindScalarAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Insert a new entry by executing OData POST request.
        /// </summary>
        /// <returns>The newly inserted entry</returns>
        Task<T> InsertEntryAsync();
        /// <summary>
        /// Insert a new entry by executing OData POST request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly inserted entry</returns>
        Task<T> InsertEntryAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Insert a new entry by executing OData POST request.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the new entry data, otherwise returns <c>null</c>.</param>
        /// <returns>The newly inserted entry</returns>
        Task<T> InsertEntryAsync(bool resultRequired);
        /// <summary>
        /// Insert a new entry by executing OData POST request.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the new entry data, otherwise returns <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly inserted entry</returns>
        Task<T> InsertEntryAsync(bool resultRequired, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the existing entry by executing OData PUT or PATCH request.
        /// </summary>
        /// <returns>The updated entry data</returns>
        Task<T> UpdateEntryAsync();
        /// <summary>
        /// Updates the existing entry by executing OData PUT or PATCH request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated entry data</returns>
        Task<T> UpdateEntryAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Updates the existing entry by executing OData PUT or PATCH request.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the updated entry data, otherwise returns <c>null</c>.</param>
        /// <returns>The updated entry data</returns>
        Task<T> UpdateEntryAsync(bool resultRequired);
        /// <summary>
        /// Updates the existing entry by executing OData PUT or PATCH request.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the updated entry data, otherwise returns <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated entry data</returns>
        Task<T> UpdateEntryAsync(bool resultRequired, CancellationToken cancellationToken);

        /// <summary>
        /// Updates entries by executing multiple OData PUT or PATCH requests.
        /// </summary>
        /// <returns>The updated entries data</returns>
        Task<IEnumerable<T>> UpdateEntriesAsync();
        /// <summary>
        /// Updates entries by executing multiple OData PUT or PATCH requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated entries data</returns>
        Task<IEnumerable<T>> UpdateEntriesAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Updates entries by executing multiple OData PUT or PATCH requests.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the updated entry data, otherwise returns <c>null</c>.</param>
        /// <returns>The updated entries data</returns>
        Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired);
        /// <summary>
        /// Updates entries by executing multiple OData PUT or PATCH requests.
        /// </summary>
        /// <param name="resultRequired">if set to <c>true</c> returns the updated entry data, otherwise returns <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated entries data</returns>
        Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the existing entry by executing OData DELETE request.
        /// </summary>
        /// <returns>Task instance.</returns>
        Task DeleteEntryAsync();
        /// <summary>
        /// Deletes the existing entry by executing OData DELETE request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task DeleteEntryAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Deletes entries by executing multiple OData DELETE requests.
        /// </summary>
        /// <returns>The number of deleted entries.</returns>
        Task<int> DeleteEntriesAsync();
        /// <summary>
        /// Deletes entries by executing multiple OData DELETE requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of deleted entries.</returns>
        Task<int> DeleteEntriesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(U linkedEntryKey);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(U linkedEntryKey, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(U linkedEntryKey, string linkName);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="linkName">Name of the link.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(U linkedEntryKey, string linkName, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey);
        /// <summary>
        /// Creates a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <param name="linkedEntryKey">The linked entry key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>();
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>(CancellationToken cancellationToken);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>(string linkName);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="linkName">Name of the link.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>(string linkName, CancellationToken cancellationToken);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="expression">The link expression.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <typeparam name="U">The linked entry type.</typeparam>
        /// <param name="expression">The link expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, CancellationToken cancellationToken);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync(ODataExpression expression);
        /// <summary>
        /// Deletes a link between entries.
        /// </summary>
        /// <param name="expression">The link expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task UnlinkEntryAsync(ODataExpression expression, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <returns>Function execution result.</returns>
        Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters);
        /// <summary>
        /// Executes the OData function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Function execution result.</returns>
        Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function and returns scalar result.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <returns>Function execution result.</returns>
        Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters);
        /// <summary>
        /// Executes the OData function and returns scalar result.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Function execution result.</returns>
        Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function and returns an array.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <returns>Function execution result.</returns>
        Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters);
        /// <summary>
        /// Executes the OData function and returns an array.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Function execution result.</returns>
        Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <returns>The command text.</returns>
        Task<string> GetCommandTextAsync();
        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The command text.</returns>
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

        /// <summary>
        /// Gets a value indicating whether the OData command filter represent the entry key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filter is an entry key; otherwise, <c>false</c>.
        /// </value>
        bool FilterIsKey { get; }

        /// <summary>
        /// Converts the current command filter value to the entry key if they are equivalent; otherwise returns <s>null</s>.
        /// </summary>
        /// <value>
        /// The filter as entry key.
        /// </value>
        IDictionary<string, object> FilterAsKey { get; }
    }
}
