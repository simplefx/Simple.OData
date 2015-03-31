using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public abstract partial class FluentClientBase<T> : IFluentClient<T> where T : class
    {
#pragma warning disable 1591

        protected readonly ODataClient _client;
        internal readonly Session _session;
        protected readonly FluentCommand _parentCommand;
        protected FluentCommand _command;
        protected readonly bool _dynamicResults;

        internal FluentClientBase(ODataClient client, Session session, FluentCommand parentCommand = null, FluentCommand command = null, bool dynamicResults = false)
        {
            _client = client;
            _session = session;
            _parentCommand = parentCommand;
            _command = command;
            _dynamicResults = dynamicResults;
        }

        internal FluentCommand Command
        {
            get
            {
                if (_command != null)
                    return _command;

                lock (this)
                {
                    return _command ?? (_command = CreateCommand());
                }
            }
        }

        protected FluentCommand CreateCommand()
        {
            return new FluentCommand(this.Session, _parentCommand);
        }

        internal Session Session
        {
            get { return _session; }
        }

        protected BoundClient<U> Link<U>(FluentCommand command, string linkName = null)
        where U : class
        {
            linkName = linkName ?? typeof(U).Name;
            var links = linkName.Split('/');
            var linkCommand = command;
            BoundClient<U> linkedClient = null;
            foreach (var link in links)
            {
                linkedClient = new BoundClient<U>(_client, _session, linkCommand, null, _dynamicResults);
                linkedClient.Command.Link(link);
                linkCommand = linkedClient.Command;
            }
            return linkedClient;
        }

        protected BoundClient<U> Link<U>(FluentCommand command, ODataExpression expression)
        where U : class
        {
            return Link<U>(command, expression.Reference);
        }

#pragma warning restore 1591

        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<U> NavigateTo<U>(string linkName = null)
            where U : class
        {
            return this.Link<U>(this.Command, linkName);
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U>> expression)
            where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <typeparam name="U">The type of the linked entity.</typeparam>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <param name="linkName">Name of the link.</param>
        /// <returns>Self.</returns>        
        public IBoundClient<IDictionary<string, object>> NavigateTo(string linkName)
        {
            return this.Link<IDictionary<string, object>>(this.Command, linkName);
        }
        /// <summary>
        /// Navigates to the linked entity.
        /// </summary>
        /// <param name="expression">The expression for the link.</param>
        /// <returns>Self.</returns>
        public IBoundClient<T> NavigateTo(ODataExpression expression)
        {
            return this.Link<T>(this.Command, expression);
        }

        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <returns>Execution result task.</returns>
        public Task ExecuteAsync()
        {
            return _client.ExecuteAsync(_command, CancellationToken.None);
        }
        /// <summary>
        /// Executes the OData function or action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result task.</returns>
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsync(_command, cancellationToken);
        }

        /// <summary>
        /// Executes the OData function or action and returns a single item.
        /// </summary>
        /// <returns>Execution result.</returns>
        public Task<T> ExecuteAsSingleAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsSingleAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }
        /// <summary>
        /// Executes the OData function or action and returns a single item.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result.</returns>
        public Task<T> ExecuteAsSingleAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsSingleAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        /// <summary>
        /// Executes the OData function or action and returns enumerable result.
        /// </summary>
        /// <returns>Execution result.</returns>
        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }
        /// <summary>
        /// Executes the OData function or action and returns enumerable result.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result.</returns>
        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        /// <summary>
        /// Executes the OData function or action and returns scalar result.
        /// </summary>
        /// <returns>Execution result.</returns>
        public Task<U> ExecuteAsScalarAsync<U>()
        {
            return _client.ExecuteAsScalarAsync<U>(_command, CancellationToken.None);
        }
        /// <summary>
        /// Executes the OData function or action and returns scalar result.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result.</returns>
        public Task<U> ExecuteAsScalarAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsScalarAsync<U>(_command, cancellationToken);
        }

        /// <summary>
        /// Executes the OData function and returns an array.
        /// </summary>
        /// <returns>Execution result.</returns>
        public Task<U[]> ExecuteAsArrayAsync<U>()
        {
            return ExecuteAsArrayAsync<U>(CancellationToken.None);
        }
        /// <summary>
        /// Executes the OData function and returns an array.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Execution result.</returns>
        public Task<U[]> ExecuteAsArrayAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsArrayAsync<U>(_command, cancellationToken);
        }

        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <returns>The command text.</returns>
        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }
        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The command text.</returns>
        public Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            return this.Command.GetCommandTextAsync(cancellationToken);
        }

#pragma warning disable 1591

        protected Task<IEnumerable<T>> RectifyColumnSelectionAsync(Task<IEnumerable<IDictionary<string, object>>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns)).ContinueWith(
                y => y.Result == null ? null : y.Result.Select(z => z.ToObject<T>(_dynamicResults)));
        }

        protected Task<T> RectifyColumnSelectionAsync(Task<IDictionary<string, object>> entry, IList<string> selectedColumns)
        {
            return entry.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns).ToObject<T>(_dynamicResults));
        }

        protected Task<Tuple<IEnumerable<T>, int>> RectifyColumnSelectionAsync(Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x =>
            {
                var result = x.Result;
                return new Tuple<IEnumerable<T>, int>(
                    RectifyColumnSelection(result.Item1, selectedColumns).Select(y => y.ToObject<T>(_dynamicResults)),
                    result.Item2);
            });
        }

        protected static IEnumerable<IDictionary<string, object>> RectifyColumnSelection(IEnumerable<IDictionary<string, object>> entries, IList<string> selectedColumns)
        {
            return entries == null ? null : entries.Select(x => RectifyColumnSelection(x, selectedColumns));
        }

        protected static IDictionary<string, object> RectifyColumnSelection(IDictionary<string, object> entry, IList<string> selectedColumns)
        {
            if (entry == null || selectedColumns == null || !selectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => selectedColumns.Any(y => IsSelectedColumn(x, y))).ToIDictionary();
            }
        }

        private static bool IsSelectedColumn(KeyValuePair<string, object> kv, string columnName)
        {
            var items = columnName.Split('/');
            if (items.Count() == 1)
            {
                return kv.Key.Homogenize() == columnName.Homogenize();
            }
            else
            {
                var item = items.First();
                return kv.Key.Homogenize() == item.Homogenize() &&
                       (kv.Value is IDictionary<string, object> && (kv.Value as IDictionary<string, object>)
                            .Any(x => IsSelectedColumn(x, string.Join("/", items.Skip(1)))) ||
                        kv.Value is IEnumerable<object> && (kv.Value as IEnumerable<object>)
                            .Any(x => x is IDictionary<string, object> && (x as IDictionary<string, object>)
                                .Any(y => IsSelectedColumn(y, string.Join("/", items.Skip(1))))));
            }
        }

        internal static IEnumerable<string> ExtractColumnNames(Expression<Func<T, object>> expression)
        {
            var lambdaExpression = Utils.CastExpressionWithTypeCheck<LambdaExpression>(expression);
            switch (lambdaExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Convert:
                    return new[] { ExtractColumnName(lambdaExpression.Body) };

                case ExpressionType.New:
                    var newExpression = lambdaExpression.Body as NewExpression;
                    return newExpression.Arguments.Select(ExtractColumnName);

                case ExpressionType.Call:
                    var callExpression = lambdaExpression.Body as MethodCallExpression;
                    if (callExpression.Method.Name == "Select" && callExpression.Arguments.Count == 2)
                    {
                        return new[] { String.Join("/", 
                            ExtractColumnName(callExpression.Arguments[0]), 
                            ExtractColumnName(callExpression.Arguments[1])) };
                    }
                    else
                    {
                        throw Utils.NotSupportedExpression(lambdaExpression.Body);
                    }

                default:
                    throw Utils.NotSupportedExpression(lambdaExpression.Body);
            }
        }

        internal static string ExtractColumnName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = expression as MemberExpression;
                    var memberName = (expression as MemberExpression).Expression.Type
                        .GetAnyProperty(memberExpression.Member.Name)
                        .GetMappedName();
                    return memberExpression.Expression is MemberExpression
                        ? String.Join("/", ExtractColumnName(memberExpression.Expression), memberName)
                        : memberName;

                case ExpressionType.Convert:
                    return ExtractColumnName((expression as UnaryExpression).Operand);

                case ExpressionType.Lambda:
                    return ExtractColumnName((expression as LambdaExpression).Body);

                case ExpressionType.Call:
                    var callExpression = expression as MethodCallExpression;
                    if (callExpression.Method.Name == "Select" && callExpression.Arguments.Count == 2)
                    {
                        return String.Join("/", 
                            ExtractColumnName(callExpression.Arguments[0]), 
                            ExtractColumnName(callExpression.Arguments[1]));
                    }
                    else
                    {
                        throw Utils.NotSupportedExpression(callExpression);
                    }

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

#pragma warning restore 1591

    }
}