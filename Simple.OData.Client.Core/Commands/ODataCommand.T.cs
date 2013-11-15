using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class ODataCommand<T> : ODataCommand, ICommand<T>
        where T : class
    {
        public ODataCommand(ODataClientWithCommand<T> client, ODataCommand parent)
            : base(client, parent)
        {
        }

        internal ODataCommand(ODataCommand ancestor)
            : base(ancestor)
        {
        }

        private ODataClientWithCommand<T> TypedClient
        {
            get
            {
                return _client as ODataClientWithCommand<T>;
            }
        }

        private ODataClientWithCommand<ODataEntry> ODataEntryClient
        {
            get
            {
                return _client as ODataClientWithCommand<ODataEntry>;
            }
        }

        internal ODataClientWithCommand<T> Client
        {
            set { _client = value; }
        }

        public new IClientWithCommand<T> For(string collectionName = null)
        {
            base.For(collectionName ?? typeof(T).Name);
            return TypedClient;
        }

        public IClientWithCommand<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            base.As(derivedCollectionName ?? typeof(U).Name);
            return new ODataClientWithCommand<U>(_client, this);
        }

        public IClientWithCommand<ODataEntry> As(ODataExpression expression)
        {
            base.As(expression.ToString());
            return new ODataClientWithCommand<ODataEntry>(_client, this);
        }

        public new IClientWithCommand<T> Key(params object[] entryKey)
        {
            base.Key(entryKey);
            return TypedClient;
        }

        public new IClientWithCommand<T> Key(IEnumerable<object> entryKey)
        {
            base.Key(entryKey);
            return TypedClient;
        }

        public new IClientWithCommand<T> Key(IDictionary<string, object> entryKey)
        {
            base.Key(entryKey);
            return TypedClient;
        }

        public new IClientWithCommand<T> Key(T entryKey)
        {
            base.Key(entryKey.ToDictionary());
            return TypedClient;
        }

        public new IClientWithCommand<T> Filter(string filter)
        {
            base.Filter(filter);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> Filter(ODataExpression expression)
        {
            base.Filter(expression);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            base.Filter(ODataExpression.FromLinqExpression(expression.Body));
            return TypedClient;
        }

        public new IClientWithCommand<T> Skip(int count)
        {
            base.Skip(count);
            return TypedClient;
        }

        public new IClientWithCommand<T> Top(int count)
        {
            base.Top(count);
            return TypedClient;
        }

        public new IClientWithCommand<T> Expand(IEnumerable<string> associations)
        {
            base.Expand(associations);
            return TypedClient;
        }

        public new IClientWithCommand<T> Expand(params string[] associations)
        {
            base.Expand(associations);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> Expand(params ODataExpression[] columns)
        {
            base.Expand(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            base.Expand(ExtractColumnNames(expression));
            return TypedClient;
        }

        public new IClientWithCommand<T> Select(IEnumerable<string> columns)
        {
            base.Select(columns);
            return TypedClient;
        }

        public new IClientWithCommand<T> Select(params string[] columns)
        {
            base.Select(columns);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> Select(params ODataExpression[] columns)
        {
            base.Select(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            base.Select(ExtractColumnNames(expression));
            return TypedClient;
        }

        public new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            base.OrderBy(columns);
            return TypedClient;
        }

        public new IClientWithCommand<T> OrderBy(params string[] columns)
        {
            base.OrderBy(columns);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> OrderBy(params ODataExpression[] columns)
        {
            base.OrderBy(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> ThenBy(params ODataExpression[] columns)
        {
            base.ThenBy(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression)
        {
            base.ThenBy(ExtractColumnNames(expression).ToArray());
            return TypedClient;
        }

        public new IClientWithCommand<T> OrderByDescending(params string[] columns)
        {
            base.OrderByDescending(columns);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> OrderByDescending(params ODataExpression[] columns)
        {
            base.OrderByDescending(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> ThenByDescending(params ODataExpression[] columns)
        {
            base.ThenByDescending(columns);
            return ODataEntryClient;
        }

        public IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            base.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return TypedClient;
        }

        public new IClientWithCommand<T> Count()
        {
            base.Count();
            return TypedClient;
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        where U : class
        {
            return TypedClient.Link<U>(this, linkName);
        }

        public new IClientWithCommand<ODataEntry> NavigateTo(ODataExpression expression)
        {
            return TypedClient.Link<ODataEntry>(this, expression.ToString());
        }

        public new IClientWithCommand<T> Set(object value)
        {
            base.Set(value);
            return TypedClient;
        }

        public new IClientWithCommand<T> Set(IDictionary<string, object> value)
        {
            base.Set(value);
            return TypedClient;
        }

        public IClientWithCommand<ODataEntry> Set(params ODataExpression[] value)
        {
            base.Set(value);
            return ODataEntryClient;
        }

        public new IClientWithCommand<T> Set(T entry)
        {
            base.Set(entry.ToDictionary());
            return TypedClient;
        }

        public new IClientWithCommand<T> Function(string functionName)
        {
            base.Function(functionName);
            return TypedClient;
        }

        public new IClientWithCommand<T> Parameters(IDictionary<string, object> parameters)
        {
            base.Parameters(parameters);
            return TypedClient;
        }
    }
}