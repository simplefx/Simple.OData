using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class ODataCommand<T> : ODataCommand, ICommand<T>
        where T : class, new()
    {
        public ODataCommand(ODataClientWithCommand<T> client, ODataCommand parent)
            : base(client, parent)
        {
        }

        internal ODataCommand(ODataCommand ancestor)
            : base(ancestor)
        {
        }

        private ODataClientWithCommand<T> CastClient
        {
            get
            {
                return _client as ODataClientWithCommand<T>;
            }
        }

        internal ODataClientWithCommand<T> Client
        {
            set { _client = value; }
        }

        public new IClientWithCommand<T> For(string collectionName = null)
        {
            base.For(collectionName ?? typeof(T).Name);
            return CastClient;
        }

        public IClientWithCommand<U> As<U>(string derivedCollectionName = null)
        where U : class, new()
        {
            base.As(derivedCollectionName ?? typeof(U).Name);
            return new ODataClientWithCommand<U>(_client, this);
        }

        public new IClientWithCommand<T> Key(params object[] entryKey)
        {
            base.Key(entryKey);
            return CastClient;
        }

        public new IClientWithCommand<T> Key(IEnumerable<object> entryKey)
        {
            base.Key(entryKey);
            return CastClient;
        }

        public new IClientWithCommand<T> Key(IDictionary<string, object> entryKey)
        {
            base.Key(entryKey);
            return CastClient;
        }

        public new IClientWithCommand<T> Key(T entryKey)
        {
            base.Key(entryKey.ToDictionary());
            return CastClient;
        }

        public new IClientWithCommand<T> Filter(string filter)
        {
            base.Filter(filter);
            return CastClient;
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            base.Filter(ODataExpression.FromLinqExpression(expression.Body));
            return CastClient;
        }

        public new IClientWithCommand<T> Skip(int count)
        {
            base.Skip(count);
            return CastClient;
        }

        public new IClientWithCommand<T> Top(int count)
        {
            base.Top(count);
            return CastClient;
        }

        public new IClientWithCommand<T> Expand(IEnumerable<string> associations)
        {
            base.Expand(associations);
            return CastClient;
        }

        public new IClientWithCommand<T> Expand(params string[] associations)
        {
            base.Expand(associations);
            return CastClient;
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            base.Expand(ExtractColumnNames(expression));
            return CastClient;
        }

        public new IClientWithCommand<T> Select(IEnumerable<string> columns)
        {
            base.Select(columns);
            return CastClient;
        }

        public new IClientWithCommand<T> Select(params string[] columns)
        {
            base.Select(columns);
            return CastClient;
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            base.Select(ExtractColumnNames(expression));
            return CastClient;
        }

        public new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            base.OrderBy(columns);
            return CastClient;
        }

        public new IClientWithCommand<T> OrderBy(params string[] columns)
        {
            base.OrderBy(columns);
            return CastClient;
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return CastClient;
        }

        public IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression)
        {
            base.ThenBy(ExtractColumnNames(expression).ToArray());
            return CastClient;
        }

        public new IClientWithCommand<T> OrderByDescending(params string[] columns)
        {
            base.ThenBy(columns);
            return CastClient;
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return CastClient;
        }

        public IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            base.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return CastClient;
        }

        public new IClientWithCommand<T> Count()
        {
            base.Count();
            return CastClient;
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        where U : class, new()
        {
            return CastClient.Link<U>(this, linkName);
        }

        public new IClientWithCommand<T> Set(object value)
        {
            base.Set(value);
            return CastClient;
        }

        public new IClientWithCommand<T> Set(T entry)
        {
            base.Set(entry.ToDictionary());
            return CastClient;
        }

        public new IClientWithCommand<T> Function(string functionName)
        {
            base.Function(functionName);
            return CastClient;
        }

        public new IClientWithCommand<T> Parameters(IDictionary<string, object> parameters)
        {
            base.Parameters(parameters);
            return CastClient;
        }
    }
}