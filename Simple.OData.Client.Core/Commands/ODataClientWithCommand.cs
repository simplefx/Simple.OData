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

        public IClientWithCommand For(ODataExpression expression)
        {
            return this.Command.For(expression);
        }

        public IClientWithCommand As(string derivedCollectionName)
        {
            return this.Command.As(derivedCollectionName);
        }

        public IClientWithCommand As(ODataExpression expression)
        {
            return this.Command.As(expression);
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

        public IClientWithCommand Filter(ODataExpression expression)
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

        public IClientWithCommand Expand(params ODataExpression[] associations)
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

        public IClientWithCommand Select(params ODataExpression[] columns)
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

        public IClientWithCommand OrderBy(params ODataExpression[] columns)
        {
            return this.Command.OrderBy(columns);
        }

        public IClientWithCommand ThenBy(params string[] columns)
        {
            return this.Command.ThenBy(columns);
        }

        public IClientWithCommand ThenBy(params ODataExpression[] columns)
        {
            return this.Command.ThenBy(columns);
        }

        public IClientWithCommand OrderByDescending(params string[] columns)
        {
            return this.Command.OrderByDescending(columns);
        }

        public IClientWithCommand OrderByDescending(params ODataExpression[] columns)
        {
            return this.Command.OrderByDescending(columns);
        }

        public IClientWithCommand ThenByDescending(params string[] columns)
        {
            return this.Command.ThenByDescending(columns);
        }

        public IClientWithCommand ThenByDescending(params ODataExpression[] columns)
        {
            return this.Command.ThenByDescending(columns);
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

        public IClientWithCommand Set(params ODataExpression[] value)
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

        public IClientWithCommand NavigateTo(ODataExpression expression)
        {
            return this.Command.NavigateTo(expression);
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
}
