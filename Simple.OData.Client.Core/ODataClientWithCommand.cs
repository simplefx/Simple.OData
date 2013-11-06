using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    // ALthough ODataClientWithCommand is never instantiated directly (only via IClientWithCommand interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason ODataCommand is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public partial class ODataClientWithCommand : IClientWithCommand
    {
        protected readonly ODataClient _client;
        protected readonly ISchema _schema;
        protected ODataCommand _parent;
        protected ODataCommand _command;

        public ODataClientWithCommand(ODataClient client, ISchema schema, ODataCommand parent = null)
        {
            _client = client;
            _schema = schema;
            _parent = parent;
        }

        protected ODataClientWithCommand(ODataClientWithCommand ancestor, ODataCommand command)
        {
            _client = ancestor._client;
            _schema = ancestor._schema;
            _parent = ancestor._parent;
            _command = command;
        }

        protected ODataCommand Command
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

        protected virtual ODataCommand CreateCommand()
        {
            return new ODataCommand(this, _parent);
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string CommandText
        {
            get { return this.Command.ToString(); }
        }

        public ODataClientWithCommand Link(ODataCommand command, string linkName)
        {
            var linkedClient = new ODataClientWithCommand(_client, _schema, command);
            linkedClient.Command.Link(linkName);
            return linkedClient;
        }

        public IClientWithCommand For(string collectionName)
        {
            return this.Command.For(collectionName);
        }

        public IClientWithCommand As(string derivedCollectionName)
        {
            return this.Command.As(derivedCollectionName);
        }

        public IClientWithCommand Key(params object[] key)
        {
            return this.Command.Key(key);
        }

        public IClientWithCommand Key(IEnumerable<object> key)
        {
            return this.Command.Key(key);
        }

        public IClientWithCommand Key(IDictionary<string, object> key)
        {
            return this.Command.Key(key);
        }

        public IClientWithCommand Filter(string filter)
        {
            return this.Command.Filter(filter);
        }

        public IClientWithCommand Filter(FilterExpression expression)
        {
            return this.Command.Filter(expression);
        }

        public IClientWithCommand Skip(int count)
        {
            return this.Command.Skip(count);
        }

        public IClientWithCommand Top(int count)
        {
            return this.Command.Top(count);
        }

        public IClientWithCommand Expand(IEnumerable<string> associations)
        {
            return this.Command.Expand(associations);
        }

        public IClientWithCommand Expand(params string[] associations)
        {
            return this.Command.Expand(associations);
        }

        public IClientWithCommand Expand(params FilterExpression[] associations)
        {
            return this.Command.Expand(associations);
        }

        public IClientWithCommand Select(IEnumerable<string> columns)
        {
            return this.Command.Select(columns);
        }

        public IClientWithCommand Select(params string[] columns)
        {
            return this.Command.Select(columns);
        }

        public IClientWithCommand Select(params FilterExpression[] columns)
        {
            return this.Command.Select(columns);
        }

        public IClientWithCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            return this.Command.OrderBy(columns);
        }

        public IClientWithCommand OrderBy(params string[] columns)
        {
            return this.Command.OrderBy(columns);
        }

        public IClientWithCommand OrderBy(params FilterExpression[] columns)
        {
            return this.Command.OrderBy(columns);
        }

        public IClientWithCommand OrderByDescending(params string[] columns)
        {
            return this.Command.OrderByDescending(columns);
        }

        public IClientWithCommand OrderByDescending(params FilterExpression[] columns)
        {
            return this.Command.OrderByDescending(columns);
        }

        public IClientWithCommand Count()
        {
            return this.Command.Count();
        }

        public IClientWithCommand Set(object value)
        {
            return this.Command.Set(value);
        }

        public IClientWithCommand Set(IDictionary<string, object> value)
        {
            return this.Command.Set(value);
        }

        public IClientWithCommand Function(string functionName)
        {
            return this.Command.Function(functionName);
        }

        public IClientWithCommand Parameters(IDictionary<string, object> parameters)
        {
            return this.Command.Parameters(parameters);
        }

        public IClientWithCommand NavigateTo(string linkName)
        {
            return this.Command.NavigateTo(linkName);
        }

        public bool FilterIsKey
        {
            get { return this.Command.FilterIsKey; }
        }

        public IDictionary<string, object> FilterAsKey
        {
            get { return this.Command.FilterAsKey; }
        }

        public IDictionary<string, object> NewValues
        {
            get { return this.Command.EntryData; }
        }
    }

    public partial class ODataClientWithCommand<T> : ODataClientWithCommand, IClientWithCommand<T>
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
        {
            return CastCommand.As<U>(derivedCollectionName);
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            return CastCommand.Filter(expression);
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            return CastCommand.Expand(expression);
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            return CastCommand.Select(expression);
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return CastCommand.OrderBy(expression);
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            return CastCommand.OrderByDescending(expression);
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        {
            return CastCommand.NavigateTo<U>(linkName);
        }
    }
}
