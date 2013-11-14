using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public partial class ODataClientWithCommand<T> : ODataClientWithCommand, IClientWithCommand<T>
        where T : class
    {
        public ODataClientWithCommand(ODataClient client, ISchema schema, ODataCommand parent = null)
            : base(client, schema, parent)
        {
        }

        internal ODataClientWithCommand(ODataClientWithCommand ancestor, ODataCommand command)
            : base(ancestor, new ODataCommand<T>(command))
        {
            CastCommand.Client = this;
        }

        protected override ODataCommand CreateCommand()
        {
            return new ODataCommand<T>(this, _parent);
        }

        private ODataCommand<T> CastCommand
        {
            get
            {
                return this.Command as ODataCommand<T>;
            }
        }

        public ODataClientWithCommand<U> Link<U>(ODataCommand command, string linkName = null)
        where U : class, new()
        {
            var linkedClient = new ODataClientWithCommand<U>(_client, _schema, command);
            linkedClient.Command.Link(linkName ?? typeof(U).Name);
            return linkedClient;
        }

        public new IClientWithCommand<T> For(string collectionName = null)
        {
            return CastCommand.For(collectionName);
        }

        public IClientWithCommand<U> As<U>(string derivedCollectionName = null)
        where U : class, new()
        {
            return CastCommand.As<U>(derivedCollectionName);
        }

        public new IClientWithCommand<T> Key(params object[] entryKey)
        {
            return CastCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(IEnumerable<object> entryKey)
        {
            return CastCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(IDictionary<string, object> entryKey)
        {
            return CastCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(T entryKey)
        {
            return CastCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Filter(string filter)
        {
            return CastCommand.Filter(filter);
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            return CastCommand.Filter(expression);
        }

        public new IClientWithCommand<T> Skip(int count)
        {
            return CastCommand.Skip(count);
        }

        public new IClientWithCommand<T> Top(int count)
        {
            return CastCommand.Top(count);
        }

        public new IClientWithCommand<T> Expand(IEnumerable<string> columns)
        {
            return CastCommand.Expand(columns);
        }

        public new IClientWithCommand<T> Expand(params string[] columns)
        {
            return CastCommand.Expand(columns);
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            return CastCommand.Expand(expression);
        }

        public new IClientWithCommand<T> Select(IEnumerable<string> columns)
        {
            return CastCommand.Select(columns);
        }

        public new IClientWithCommand<T> Select(params string[] columns)
        {
            return CastCommand.Select(columns);
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            return CastCommand.Select(expression);
        }

        public new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            return CastCommand.OrderBy(columns);
        }

        public new IClientWithCommand<T> OrderBy(params string[] columns)
        {
            return CastCommand.OrderBy(columns);
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return CastCommand.OrderBy(expression);
        }

        public IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression)
        {
            return CastCommand.ThenBy(expression);
        }

        public new IClientWithCommand<T> OrderByDescending(params string[] columns)
        {
            return CastCommand.OrderByDescending(columns);
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            return CastCommand.OrderByDescending(expression);
        }

        public IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            return CastCommand.ThenByDescending(expression);
        }

        public new IClientWithCommand<T> Count()
        {
            return CastCommand.Count();
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        where U : class, new()
        {
            return CastCommand.NavigateTo<U>(linkName);
        }

        public new IClientWithCommand<T> Set(object value)
        {
            return CastCommand.Set(value);
        }

        public IClientWithCommand<T> Set(T entry)
        {
            return CastCommand.Set(entry);
        }

        public new IClientWithCommand<T> Function(string functionName)
        {
            return CastCommand.Function(functionName);
        }

        public new IClientWithCommand<T> Parameters(IDictionary<string, object> parameters)
        {
            return CastCommand.Parameters(parameters);
        }
    }
}