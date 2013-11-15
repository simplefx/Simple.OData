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
            TypedCommand.Client = this;
        }

        protected override ODataCommand CreateCommand()
        {
            return new ODataCommand<T>(this, _parent);
        }

        private ODataCommand<T> TypedCommand
        {
            get
            {
                return this.Command as ODataCommand<T>;
            }
        }

        private ODataCommand<ODataEntry> ODataEntryCommand
        {
            get
            {
                return this.Command as ODataCommand<ODataEntry>;
            }
        }

        public ODataClientWithCommand<U> Link<U>(ODataCommand command, string linkName = null)
        where U : class
        {
            var linkedClient = new ODataClientWithCommand<U>(_client, _schema, command);
            linkedClient.Command.Link(linkName ?? typeof(U).Name);
            return linkedClient;
        }

        public new IClientWithCommand<T> For(string collectionName = null)
        {
            return TypedCommand.For(collectionName);
        }

        public IClientWithCommand<ODataEntry> For(ODataExpression expression)
        {
            return ODataEntryCommand.For(expression.Reference);
        }

        public IClientWithCommand<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            return TypedCommand.As<U>(derivedCollectionName);
        }

        public IClientWithCommand<ODataEntry> As(ODataExpression expression)
        {
            return TypedCommand.As(expression);
        }

        public new IClientWithCommand<T> Key(params object[] entryKey)
        {
            return TypedCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(IEnumerable<object> entryKey)
        {
            return TypedCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(IDictionary<string, object> entryKey)
        {
            return TypedCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Key(T entryKey)
        {
            return TypedCommand.Key(entryKey);
        }

        public new IClientWithCommand<T> Filter(string filter)
        {
            return TypedCommand.Filter(filter);
        }

        public IClientWithCommand<ODataEntry> Filter(ODataExpression expression)
        {
            return TypedCommand.Filter(expression);
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            return TypedCommand.Filter(expression);
        }

        public new IClientWithCommand<T> Skip(int count)
        {
            return TypedCommand.Skip(count);
        }

        public new IClientWithCommand<T> Top(int count)
        {
            return TypedCommand.Top(count);
        }

        public new IClientWithCommand<T> Expand(IEnumerable<string> columns)
        {
            return TypedCommand.Expand(columns);
        }

        public new IClientWithCommand<T> Expand(params string[] columns)
        {
            return TypedCommand.Expand(columns);
        }

        public IClientWithCommand<ODataEntry> Expand(params ODataExpression[] associations)
        {
            return TypedCommand.Expand(associations);
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            return TypedCommand.Expand(expression);
        }

        public new IClientWithCommand<T> Select(IEnumerable<string> columns)
        {
            return TypedCommand.Select(columns);
        }

        public new IClientWithCommand<T> Select(params string[] columns)
        {
            return TypedCommand.Select(columns);
        }

        public IClientWithCommand<ODataEntry> Select(params ODataExpression[] columns)
        {
            return TypedCommand.Select(columns);
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            return TypedCommand.Select(expression);
        }

        public new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            return TypedCommand.OrderBy(columns);
        }

        public new IClientWithCommand<T> OrderBy(params string[] columns)
        {
            return TypedCommand.OrderBy(columns);
        }

        public IClientWithCommand<ODataEntry> OrderBy(params ODataExpression[] columns)
        {
            return TypedCommand.OrderBy(columns);
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return TypedCommand.OrderBy(expression);
        }

        public IClientWithCommand<ODataEntry> ThenBy(params ODataExpression[] columns)
        {
            return TypedCommand.ThenBy(columns);
        }

        public IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression)
        {
            return TypedCommand.ThenBy(expression);
        }

        public new IClientWithCommand<T> OrderByDescending(params string[] columns)
        {
            return TypedCommand.OrderByDescending(columns);
        }

        public IClientWithCommand<ODataEntry> OrderByDescending(params ODataExpression[] columns)
        {
            return TypedCommand.OrderByDescending(columns);
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            return TypedCommand.OrderByDescending(expression);
        }

        public IClientWithCommand<ODataEntry> ThenByDescending(params ODataExpression[] columns)
        {
            return TypedCommand.ThenByDescending(columns);
        }

        public IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            return TypedCommand.ThenByDescending(expression);
        }

        public new IClientWithCommand<T> Count()
        {
            return TypedCommand.Count();
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        where U : class
        {
            return TypedCommand.NavigateTo<U>(linkName);
        }

        public new IClientWithCommand<ODataEntry> NavigateTo(ODataExpression expression)
        {
            return TypedCommand.NavigateTo(expression);
        }

        public new IClientWithCommand<T> Set(object value)
        {
            return TypedCommand.Set(value);
        }

        public new IClientWithCommand<T> Set(IDictionary<string, object> value)
        {
            return TypedCommand.Set(value);
        }

        public IClientWithCommand<ODataEntry> Set(params ODataExpression[] value)
        {
            return TypedCommand.Set(value);
        }

        public IClientWithCommand<T> Set(T entry)
        {
            return TypedCommand.Set(entry);
        }

        public new IClientWithCommand<T> Function(string functionName)
        {
            return TypedCommand.Function(functionName);
        }

        public new IClientWithCommand<T> Parameters(IDictionary<string, object> parameters)
        {
            return TypedCommand.Parameters(parameters);
        }
    }
}