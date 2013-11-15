using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public partial class FluentClient<T> : FluentClient, IFluentClient<T>
        where T : class
    {
        public FluentClient(ODataClient client, ISchema schema, ODataCommand parent = null)
            : base(client, schema, parent)
        {
        }

        internal FluentClient(FluentClient ancestor, ODataCommand command)
            : base(ancestor, new ODataCommand<T>(command))
        {
        }

        protected override ODataCommand CreateCommand()
        {
            return new ODataCommand<T>(this.Schema, _parent);
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

        public FluentClient<U> Link<U>(ODataCommand command, string linkName = null)
        where U : class
        {
            var linkedClient = new FluentClient<U>(_client, _schema, command);
            linkedClient.Command.Link(linkName ?? typeof(U).Name);
            return linkedClient;
        }

        public new IFluentClient<T> For(string collectionName = null)
        {
            this.TypedCommand.For(collectionName ?? typeof(T).Name);
            return this;
        }

        public IFluentClient<ODataEntry> For(ODataExpression expression)
        {
            this.ODataEntryCommand.For(expression.Reference);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            this.TypedCommand.As(derivedCollectionName ?? typeof(U).Name);
            return new FluentClient<U>(this, this.TypedCommand);
        }

        public IFluentClient<ODataEntry> As(ODataExpression expression)
        {
            this.TypedCommand.As(expression.ToString());
            return new FluentClient<ODataEntry>(this, this.TypedCommand);
        }

        public new IFluentClient<T> Key(params object[] entryKey)
        {
            this.TypedCommand.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Key(IEnumerable<object> entryKey)
        {
            this.TypedCommand.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Key(IDictionary<string, object> entryKey)
        {
            this.TypedCommand.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Key(T entryKey)
        {
            this.TypedCommand.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Filter(string filter)
        {
            this.TypedCommand.Filter(filter);
            return this;
        }

        public IFluentClient<ODataEntry> Filter(ODataExpression expression)
        {
            this.TypedCommand.Filter(expression);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Filter(Expression<Func<T, bool>> expression)
        {
            this.TypedCommand.Filter(expression);
            return this;
        }

        public new IFluentClient<T> Skip(int count)
        {
            this.TypedCommand.Skip(count);
            return this;
        }

        public new IFluentClient<T> Top(int count)
        {
            this.TypedCommand.Top(count);
            return this;
        }

        public new IFluentClient<T> Expand(IEnumerable<string> columns)
        {
            this.TypedCommand.Expand(columns);
            return this;
        }

        public new IFluentClient<T> Expand(params string[] columns)
        {
            this.TypedCommand.Expand(columns);
            return this;
        }

        public IFluentClient<ODataEntry> Expand(params ODataExpression[] associations)
        {
            this.TypedCommand.Expand(associations);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Expand(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.Expand(expression);
            return this;
        }

        public new IFluentClient<T> Select(IEnumerable<string> columns)
        {
            this.TypedCommand.Select(columns);
            return this;
        }

        public new IFluentClient<T> Select(params string[] columns)
        {
            this.TypedCommand.Select(columns);
            return this;
        }

        public IFluentClient<ODataEntry> Select(params ODataExpression[] columns)
        {
            this.TypedCommand.Select(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Select(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.Select(expression);
            return this;
        }

        public new IFluentClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.TypedCommand.OrderBy(columns);
            return this;
        }

        public new IFluentClient<T> OrderBy(params string[] columns)
        {
            this.TypedCommand.OrderBy(columns);
            return this;
        }

        public IFluentClient<ODataEntry> OrderBy(params ODataExpression[] columns)
        {
            this.TypedCommand.OrderBy(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> OrderBy(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.OrderBy(expression);
            return this;
        }

        public IFluentClient<ODataEntry> ThenBy(params ODataExpression[] columns)
        {
            this.TypedCommand.ThenBy(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.ThenBy(expression);
            return this;
        }

        public new IFluentClient<T> OrderByDescending(params string[] columns)
        {
            this.TypedCommand.OrderByDescending(columns);
            return this;
        }

        public IFluentClient<ODataEntry> OrderByDescending(params ODataExpression[] columns)
        {
            this.TypedCommand.OrderByDescending(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.OrderByDescending(expression);
            return this;
        }

        public IFluentClient<ODataEntry> ThenByDescending(params ODataExpression[] columns)
        {
            this.TypedCommand.ThenByDescending(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.TypedCommand.ThenByDescending(expression);
            return this;
        }

        public new IFluentClient<T> Count()
        {
            this.TypedCommand.Count();
            return this;
        }

        public IFluentClient<U> NavigateTo<U>(string linkName = null)
        where U : class
        {
            return this.Link<U>(this.TypedCommand, linkName);
        }

        public new IFluentClient<ODataEntry> NavigateTo(ODataExpression expression)
        {
            return this.Link<ODataEntry>(this.TypedCommand, expression.ToString());
        }

        public new IFluentClient<T> Set(object value)
        {
            this.TypedCommand.Set(value);
            return this;
        }

        public new IFluentClient<T> Set(IDictionary<string, object> value)
        {
            this.TypedCommand.Set(value);
            return this;
        }

        public IFluentClient<ODataEntry> Set(params ODataExpression[] value)
        {
            this.TypedCommand.Set(value);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Set(T entry)
        {
            this.TypedCommand.Set(entry);
            return this;
        }

        public new IFluentClient<T> Function(string functionName)
        {
            this.TypedCommand.Function(functionName);
            return this;
        }

        public new IFluentClient<T> Parameters(IDictionary<string, object> parameters)
        {
            this.TypedCommand.Parameters(parameters);
            return this;
        }
    }
}