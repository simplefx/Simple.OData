using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    // ALthough BoundClient is never instantiated directly (only via IBoundClient interface)
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
            this.Command.Expand(ColumnExpression.ExtractColumnNames(expression));
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
            this.Command.Select(ColumnExpression.ExtractColumnNames(expression));
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
            this.Command.OrderBy(ColumnExpression.ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public IBoundClient<T> ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
            return this;
        }

        public IBoundClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.Command.ThenBy(ColumnExpression.ExtractColumnNames(expression).ToArray());
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
            this.Command.OrderBy(ColumnExpression.ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public IBoundClient<T> ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
            return this;
        }

        public IBoundClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.ThenByDescending(ColumnExpression.ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IBoundClient<T> Count()
        {
            this.Command.Count();
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

        private BoundClient<ODataEntry> CreateClientForODataEntry() 
        {
            return new BoundClient<ODataEntry>(_client, _session, _parentCommand, this.Command, true); ;
        }
    }
}
