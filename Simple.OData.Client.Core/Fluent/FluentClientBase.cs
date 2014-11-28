using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public abstract partial class FluentClientBase<T> : IFluentClient<T> where T : class
    {
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

        public IBoundClient<U> NavigateTo<U>(string linkName = null)
            where U : class
        {
            return this.Link<U>(this.Command, linkName);
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U>> expression)
            where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<IDictionary<string, object>> NavigateTo(string linkName)
        {
            return this.Link<IDictionary<string, object>>(this.Command, linkName);
        }

        public IBoundClient<T> NavigateTo(ODataExpression expression)
        {
            return this.Link<T>(this.Command, expression);
        }

        public Task<T> ExecuteAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }

        public Task<T> ExecuteAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync()
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, CancellationToken.None), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> ExecuteAsEnumerableAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteAsEnumerableAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<U> ExecuteAsScalarAsync<U>()
        {
            return _client.ExecuteAsScalarAsync<U>(_command, CancellationToken.None);
        }

        public Task<U> ExecuteAsScalarAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsScalarAsync<U>(_command, cancellationToken);
        }

        public Task<U[]> ExecuteAsArrayAsync<U>()
        {
            return ExecuteAsArrayAsync<U>(CancellationToken.None);
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            return this.Command.GetCommandTextAsync(cancellationToken);
        }

        public Task<U[]> ExecuteAsArrayAsync<U>(CancellationToken cancellationToken)
        {
            return _client.ExecuteAsArrayAsync<U>(_command, cancellationToken);
        }

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
            if (selectedColumns == null || !selectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => selectedColumns.Any(y => x.Key.Homogenize() == y.Homogenize())).ToIDictionary();
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
                    var memberName = memberExpression.Member.Name;
                    return memberExpression.Expression is MemberExpression
                        ? String.Join("/", ExtractColumnName(memberExpression.Expression), memberName)
                        : memberName;

                case ExpressionType.Convert:
                    return ExtractColumnName((expression as UnaryExpression).Operand);

                case ExpressionType.Lambda:
                    return ExtractColumnName((expression as LambdaExpression).Body);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }
    }
}