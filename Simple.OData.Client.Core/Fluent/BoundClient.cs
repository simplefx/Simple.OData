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

    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    public partial class BoundClient<T> : FluentClientBase<T>, IBoundClient<T>
        where T : class
    {
        internal BoundClient(ODataClient client, Session session, FluentCommand parentCommand = null, FluentCommand command = null, bool dynamicResults = false)
            : base(client, session, parentCommand, command, dynamicResults)
        {
        }

        #pragma warning disable 1591

        private BoundClient<U> Link<U>(FluentCommand command, string linkName = null)
        where U : class
        {
            linkName = linkName ?? typeof (U).Name;
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

        private BoundClient<U> Link<U>(FluentCommand command, ODataExpression expression)
        where U : class
        {
            return Link<U>(command, expression.Reference);
        }

        public IBoundClient<T> For(string collectionName = null)
        {
            this.Command.For(collectionName ?? typeof(T).Name);
            return this;
        }

        public IBoundClient<ODataEntry> For(ODataExpression expression)
        {
            this.Command.For(expression.Reference);
            return CreateClientForODataEntry();
        }

        public IBoundClient<IDictionary<string, object>> As(string derivedCollectionName)
        {
            this.Command.As(derivedCollectionName);
            return new BoundClient<IDictionary<string, object>>(_client, _session, _parentCommand, this.Command, _dynamicResults);
        }

        public IBoundClient<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            this.Command.As(derivedCollectionName ?? typeof(U).Name);
            return new BoundClient<U>(_client, _session, _parentCommand, this.Command, _dynamicResults);
        }

        public IBoundClient<ODataEntry> As(ODataExpression expression)
        {
            this.Command.As(expression);
            return CreateClientForODataEntry();
        }

        public IBoundClient<T> Key(params object[] entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IBoundClient<T> Key(IEnumerable<object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IBoundClient<T> Key(IDictionary<string, object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IBoundClient<T> Key(T entryKey)
        {
            this.Command.Key(entryKey.ToDictionary());
            return this;
        }

        public IBoundClient<T> Filter(string filter)
        {
            this.Command.Filter(filter);
            return this;
        }

        public IBoundClient<T> Filter(ODataExpression expression)
        {
            this.Command.Filter(expression);
            return this;
        }

        public IBoundClient<T> Filter(Expression<Func<T, bool>> expression)
        {
            this.Command.Filter(ODataExpression.FromLinqExpression(expression.Body));
            return this;
        }

        public IBoundClient<T> Skip(int count)
        {
            this.Command.Skip(count);
            return this;
        }

        public IBoundClient<T> Top(int count)
        {
            this.Command.Top(count);
            return this;
        }

        public IBoundClient<T> Expand(IEnumerable<string> columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IBoundClient<T> Expand(params string[] columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IBoundClient<T> Expand(params ODataExpression[] associations)
        {
            this.Command.Expand(associations);
            return this;
        }

        public IBoundClient<T> Expand(Expression<Func<T, object>> expression)
        {
            this.Command.Expand(ExtractColumnNames(expression));
            return this;
        }

        public IBoundClient<T> Select(IEnumerable<string> columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IBoundClient<T> Select(params string[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IBoundClient<T> Select(params ODataExpression[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IBoundClient<T> Select(Expression<Func<T, object>> expression)
        {
            this.Command.Select(ExtractColumnNames(expression));
            return this;
        }

        public IBoundClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IBoundClient<T> OrderBy(params string[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IBoundClient<T> OrderBy(params ODataExpression[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IBoundClient<T> OrderBy(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public IBoundClient<T> ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
            return this;
        }

        public IBoundClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.Command.ThenBy(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IBoundClient<T> OrderByDescending(params string[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IBoundClient<T> OrderByDescending(params ODataExpression[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IBoundClient<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public IBoundClient<T> ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
            return this;
        }

        public IBoundClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IBoundClient<T> Count()
        {
            this.Command.Count();
            return this;
        }

        public IBoundClient<T> Set(object value)
        {
            this.Command.Set(value);
            return this;
        }

        public IBoundClient<T> Set(IDictionary<string, object> value)
        {
            this.Command.Set(value);
            return this;
        }

        public IBoundClient<T> Set(params ODataExpression[] value)
        {
            this.Command.Set(value);
            return this;
        }

        public IBoundClient<T> Set(T entry)
        {
            this.Command.Set(entry);
            return this;
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

        public IBoundClient<T> Function(string functionName)
        {
            this.Command.Function(functionName);
            return this;
        }

        public IBoundClient<T> Action(string actionName)
        {
            this.Command.Action(actionName);
            return this;
        }

        public bool FilterIsKey
        {
            get { return this.Command.FilterIsKey; }
        }

        public IDictionary<string, object> FilterAsKey
        {
            get { return this.Command.FilterAsKey; }
        }

        #pragma warning restore 1591

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

        private BoundClient<ODataEntry> CreateClientForODataEntry() 
        {
            return new BoundClient<ODataEntry>(_client, _session, _parentCommand, this.Command, true); ;
        }
    }
}
