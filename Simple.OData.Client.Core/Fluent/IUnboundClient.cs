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
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IUnboundClient<T> : IFluentClient<T> 
        where T : class
    {
        /// <summary>
        /// Sets the OData function name.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Function(string functionName);
        /// <summary>
        /// Sets the OData action name.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Action(string actionName);

        /// <summary>
        /// Sets the specified entry value for update.
        /// </summary>
        /// <param name="value">The value to update the entry with.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Set(object value);
        /// <summary>
        /// Sets the specified entry value for update.
        /// </summary>
        /// <param name="value">The value to update the entry with.</param>
        /// <returns></returns>
        IUnboundClient<T> Set(IDictionary<string, object> value);
        /// <summary>
        /// Sets the specified entry value for update.
        /// </summary>
        /// <param name="value">The value to update the entry with.</param>
        /// <returns></returns>
        IUnboundClient<T> Set(params ODataExpression[] value);

        /// <summary>
        /// Skips the specified number of entries from the result.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Skip(int count);

        /// <summary>
        /// Limits the number of results with the specified value.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Top(int count);

        /// <summary>
        /// Expands the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Expand(IEnumerable<string> associations);
        /// <summary>
        /// Expands the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Expand(params string[] associations);
        /// <summary>
        /// Expands the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Expand(params ODataExpression[] associations);
        /// <summary>
        /// Expands the specified expression.
        /// </summary>
        /// <param name="expression">The expression for associations to expand.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Expand(Expression<Func<T, object>> expression);

        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Select(IEnumerable<string> columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Select(params string[] columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Select(params ODataExpression[] columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="expression">The expression for the selected columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> Select(Expression<Func<T, object>> expression);

        /// <summary>
        /// Sorts the result by the specified columns in the specified order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderBy(params string[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderBy(params ODataExpression[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderBy(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> ThenBy(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderByDescending(params string[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderByDescending(params ODataExpression[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> OrderByDescending(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        IUnboundClient<T> ThenByDescending(Expression<Func<T, object>> expression);

        /// <summary>
        /// Requests the number of results.
        /// </summary>
        /// <returns>Self.</returns>
        IUnboundClient<T> Count();

        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(string linkName = null) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, U>> expression) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<IDictionary<string, object>> NavigateTo(string linkName);
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<T> NavigateTo(ODataExpression expression);

        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <returns>Execution result.</returns>
        Task<T> ExecuteAsync();
        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action execution result.</returns>
        Task<T> ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function or action and returns collection.
        /// </summary>
        /// <returns>Action execution result.</returns>
        Task<IEnumerable<T>> ExecuteAsEnumerableAsync();
        /// <summary>
        /// Executes the OData function or action and returns collection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result.</returns>
        Task<IEnumerable<T>> ExecuteAsEnumerableAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function or action and returns scalar result.
        /// </summary>
        /// <typeparam name="U">The type of the result.</typeparam>
        /// <returns>Execution result.</returns>
        Task<U> ExecuteAsScalarAsync<U>();
        /// <summary>
        /// Executes the OData function or action and returns scalar result.
        /// </summary>
        /// <typeparam name="U">The type of the result.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action execution result.</returns>
        Task<U> ExecuteAsScalarAsync<U>(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function or action and returns an array.
        /// </summary>
        /// <typeparam name="U">The type of the result array.</typeparam>
        /// <returns>Execution result.</returns>
        Task<U[]> ExecuteAsArrayAsync<U>();
        /// <summary>
        /// Executes the OData function or action and returns an array.
        /// </summary>
        /// <typeparam name="T">The type of the result array.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action execution result.</returns>
        Task<U[]> ExecuteAsArrayAsync<U>(CancellationToken cancellationToken);

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
    }
}