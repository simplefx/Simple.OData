using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    // ALthough FluentFluentClient is never instantiated directly (only via IFluentClient interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason ODataCommand is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public partial class FluentClient
    {
        protected readonly ODataClient _client;
        protected readonly ISchema _schema;
        protected ODataCommand _parent;
        protected ODataCommand _command;

        public FluentClient(ODataClient client, ODataCommand parent = null)
        {
            _client = client;
            _schema = client.Schema;
            _parent = parent;
        }

        protected FluentClient(FluentClient ancestor, ODataCommand command)
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
            return new ODataCommand(this.Schema, _parent);
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string CommandText
        {
            get { return this.Command.ToString(); }
        }

        public FluentClient Link(ODataCommand command, string linkName)
        {
            var linkedClient = new FluentClient(_client, command);
            linkedClient.Command.Link(linkName);
            return linkedClient;
        }

        public void For(string collectionName)
        {
            this.Command.For(collectionName);
        }

        public void For(ODataExpression expression)
        {
            this.Command.For(expression);
        }

        public void As(string derivedCollectionName)
        {
            this.Command.As(derivedCollectionName);
        }

        public void As(ODataExpression expression)
        {
            this.Command.As(expression);
        }

        public void Key(params object[] key)
        {
            this.Command.Key(key);
        }

        public void Key(IEnumerable<object> key)
        {
            this.Command.Key(key);
        }

        public void Key(IDictionary<string, object> key)
        {
            this.Command.Key(key);
        }

        public void Filter(string filter)
        {
            this.Command.Filter(filter);
        }

        public void Filter(ODataExpression expression)
        {
            this.Command.Filter(expression);
        }

        public void Skip(int count)
        {
            this.Command.Skip(count);
        }

        public void Top(int count)
        {
            this.Command.Top(count);
        }

        public void Expand(IEnumerable<string> associations)
        {
            this.Command.Expand(associations);
        }

        public void Expand(params string[] associations)
        {
            this.Command.Expand(associations);
        }

        public void Expand(params ODataExpression[] associations)
        {
            this.Command.Expand(associations);
        }

        public void Select(IEnumerable<string> columns)
        {
            this.Command.Select(columns);
        }

        public void Select(params string[] columns)
        {
            this.Command.Select(columns);
        }

        public void Select(params ODataExpression[] columns)
        {
            this.Command.Select(columns);
        }

        public void OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.Command.OrderBy(columns);
        }

        public void OrderBy(params string[] columns)
        {
            this.Command.OrderBy(columns);
        }

        public void OrderBy(params ODataExpression[] columns)
        {
            this.Command.OrderBy(columns);
        }

        public void ThenBy(params string[] columns)
        {
            this.Command.ThenBy(columns);
        }

        public void ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
        }

        public void OrderByDescending(params string[] columns)
        {
            this.Command.OrderByDescending(columns);
        }

        public void OrderByDescending(params ODataExpression[] columns)
        {
            this.Command.OrderByDescending(columns);
        }

        public void ThenByDescending(params string[] columns)
        {
            this.Command.ThenByDescending(columns);
        }

        public void ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
        }

        public void Count()
        {
            this.Command.Count();
        }

        public void Set(object value)
        {
            this.Command.Set(value);
        }

        public void Set(IDictionary<string, object> value)
        {
            this.Command.Set(value);
        }

        public void Set(params ODataExpression[] value)
        {
            this.Command.Set(value);
        }

        public void Function(string functionName)
        {
            this.Command.Function(functionName);
        }

        public void Parameters(IDictionary<string, object> parameters)
        {
            this.Command.Parameters(parameters);
        }

        public void NavigateTo(string linkName)
        {
            Link(this.Command, linkName);
        }

        public void NavigateTo(ODataExpression expression)
        {
            Link(this.Command, expression.ToString());
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
