using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    // ALthough FluentFluentClient is never instantiated directly (only via IFluentClient interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason FluentCommand is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public partial class FluentClient<T> : IFluentClient<T>
        where T : class
    {
        private readonly ODataClient _client;
        private readonly ISchema _schema;
        private readonly FluentCommand _parentCommand;
        private FluentCommand _command;
        private readonly bool _dynamicResults;

        internal FluentClient(ODataClient client, ISchema schema, FluentCommand parentCommand = null, FluentCommand command = null, bool dynamicResults = false)
        {
            _client = client;
            _schema = schema;
            _parentCommand = parentCommand;
            _command = command;
            _dynamicResults = dynamicResults;
        }

        private FluentCommand Command
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

        private FluentCommand CreateCommand()
        {
            return new FluentCommand(this.Schema, _parentCommand);
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public FluentClient<U> Link<U>(FluentCommand command, string linkName = null)
        where U : class
        {
            linkName = linkName ?? typeof (U).Name;
            var links = linkName.Split('/');
            var linkCommand = command;
            FluentClient<U> linkedClient = null;
            foreach (var link in links)
            {
                linkedClient = new FluentClient<U>(_client, _schema, linkCommand, null, _dynamicResults);
                linkedClient.Command.Link(link);
                linkCommand = linkedClient.Command;
            }
            return linkedClient;
        }

        public FluentClient<U> Link<U>(FluentCommand command, ODataExpression expression)
        where U : class
        {
            return Link<U>(command, expression.Reference);
        }

        public IFluentClient<T> For(string collectionName = null)
        {
            this.Command.For(collectionName ?? typeof(T).Name);
            return this;
        }

        public IFluentClient<ODataEntry> For(ODataExpression expression)
        {
            this.Command.For(expression.Reference);
            return CreateClientForODataEntry();
        }

        public IFluentClient<IDictionary<string, object>> As(string derivedCollectionName)
        {
            this.Command.As(derivedCollectionName);
            return new FluentClient<IDictionary<string, object>>(_client, _schema, _parentCommand, this.Command, _dynamicResults);
        }

        public IFluentClient<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            this.Command.As(derivedCollectionName ?? typeof(U).Name);
            return new FluentClient<U>(_client, _schema, _parentCommand, this.Command, _dynamicResults);
        }

        public IFluentClient<ODataEntry> As(ODataExpression expression)
        {
            this.Command.As(expression);
            return CreateClientForODataEntry();
        }

        public IFluentClient<T> Key(params object[] entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IFluentClient<T> Key(IEnumerable<object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IFluentClient<T> Key(IDictionary<string, object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IFluentClient<T> Key(T entryKey)
        {
            this.Command.Key(entryKey.ToDictionary());
            return this;
        }

        public IFluentClient<T> Filter(string filter)
        {
            this.Command.Filter(filter);
            return this;
        }

        public IFluentClient<T> Filter(ODataExpression expression)
        {
            this.Command.Filter(expression);
            return this;
        }

        public IFluentClient<T> Filter(Expression<Func<T, bool>> expression)
        {
            this.Command.Filter(ODataExpression.FromLinqExpression(expression.Body));
            return this;
        }

        public IFluentClient<T> Skip(int count)
        {
            this.Command.Skip(count);
            return this;
        }

        public IFluentClient<T> Top(int count)
        {
            this.Command.Top(count);
            return this;
        }

        public IFluentClient<T> Expand(IEnumerable<string> columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IFluentClient<T> Expand(params string[] columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IFluentClient<T> Expand(params ODataExpression[] associations)
        {
            this.Command.Expand(associations);
            return this;
        }

        public IFluentClient<T> Expand(Expression<Func<T, object>> expression)
        {
            this.Command.Expand(ExtractColumnNames(expression));
            return this;
        }

        public IFluentClient<T> Select(IEnumerable<string> columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IFluentClient<T> Select(params string[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IFluentClient<T> Select(params ODataExpression[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IFluentClient<T> Select(Expression<Func<T, object>> expression)
        {
            this.Command.Select(ExtractColumnNames(expression));
            return this;
        }

        public IFluentClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IFluentClient<T> OrderBy(params string[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IFluentClient<T> OrderBy(params ODataExpression[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IFluentClient<T> OrderBy(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public IFluentClient<T> ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
            return this;
        }

        public IFluentClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.Command.ThenBy(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IFluentClient<T> OrderByDescending(params string[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IFluentClient<T> OrderByDescending(params ODataExpression[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IFluentClient<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public IFluentClient<T> ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
            return this;
        }

        public IFluentClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IFluentClient<T> Count()
        {
            this.Command.Count();
            return this;
        }

        public IFluentClient<T> Set(object value)
        {
            this.Command.Set(value);
            return this;
        }

        public IFluentClient<T> Set(IDictionary<string, object> value)
        {
            this.Command.Set(value);
            return this;
        }

        public IFluentClient<T> Set(params ODataExpression[] value)
        {
            this.Command.Set(value);
            return this;
        }

        public IFluentClient<T> Set(T entry)
        {
            this.Command.Set(entry);
            return this;
        }

        public IFluentClient<U> NavigateTo<U>(string linkName = null)
            where U : class
        {
            return this.Link<U>(this.Command, linkName);
        }

        public IFluentClient<U> NavigateTo<U>(Expression<Func<T, U>> expression)
            where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IFluentClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IFluentClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IFluentClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IFluentClient<IDictionary<string, object>> NavigateTo(string linkName)
        {
            return this.Link<IDictionary<string, object>>(this.Command, linkName);
        }

        public IFluentClient<T> NavigateTo(ODataExpression expression)
        {
            return this.Link<T>(this.Command, expression);
        }

        public IFluentClient<T> Function(string functionName)
        {
            this.Command.Function(functionName);
            return this;
        }

        public IFluentClient<T> Parameters(IDictionary<string, object> parameters)
        {
            this.Command.Parameters(parameters);
            return this;
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
                        ? string.Join("/", ExtractColumnName(memberExpression.Expression), memberName)
                        : memberName;

                case ExpressionType.Convert:
                    return ExtractColumnName((expression as UnaryExpression).Operand);

                case ExpressionType.Lambda:
                    return ExtractColumnName((expression as LambdaExpression).Body);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

        public bool FilterIsKey
        {
            get { return this.Command.FilterIsKey; }
        }

        public IDictionary<string, object> FilterAsKey
        {
            get { return this.Command.FilterAsKey; }
        }

        private FluentClient<ODataEntry> CreateClientForODataEntry() 
        {
            return new FluentClient<ODataEntry>(_client, _schema, _parentCommand, this.Command, true); ;
        }
    }
}
