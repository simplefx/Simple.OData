using System.Collections.Generic;
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