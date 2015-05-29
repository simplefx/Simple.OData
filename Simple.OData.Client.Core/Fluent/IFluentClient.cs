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
    public interface IFluentClient<T>
        where T : class
    {
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
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, ISet<U>>> expression) where U : class;
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        IBoundClient<U> NavigateTo<U>(Expression<Func<T, HashSet<U>>> expression) where U : class;
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
        Task ExecuteAsync();
        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action execution result.</returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <returns>Execution result.</returns>
        Task<T> ExecuteAsSingleAsync();
        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action execution result.</returns>
        Task<T> ExecuteAsSingleAsync(CancellationToken cancellationToken);

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
        /// <typeparam name="U">The type of the result array.</typeparam>
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
