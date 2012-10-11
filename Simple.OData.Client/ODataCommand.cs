using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Simple.OData.Client
{
    class ODataCommand : ICommand
    {
        private ODataClientWithCommand _client;
        private ODataCommand _parent;
        private string _collectionName;
        private IDictionary<string, object> _key;
        private string _filter;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        private ODataClientWithCommand _navigateTo;

        public ODataCommand(ODataClientWithCommand client, ODataCommand parent)
        {
            _client = client;
            _parent = parent;
        }

        public IClientWithCommand Collection(string collectionName)
        {
            _collectionName = collectionName;
            return _client;
        }

        public IClientWithCommand Get(IDictionary<string, object> key)
        {
            _key = key;
            return _client;
        }

        public IClientWithCommand Filter(string filter)
        {
            _filter = filter;
            return _client;
        }

        public IClientWithCommand Skip(int count)
        {
            _skipCount = count;
            return _client;
        }

        public IClientWithCommand Top(int count)
        {
            _topCount = count;
            return _client;
        }

        public IClientWithCommand Expand(IEnumerable<string> associations)
        {
            _expandAssociations = associations.ToList();
            return _client;
        }

        public IClientWithCommand Expand(params string[] associations)
        {
            _expandAssociations = associations.ToList();
            return _client;
        }

        public IClientWithCommand Select(IEnumerable<string> columns)
        {
            _selectColumns = columns.ToList();
            return _client;
        }

        public IClientWithCommand Select(params string[] columns)
        {
            _selectColumns = columns.ToList();
            return _client;
        }

        public IClientWithCommand OrderBy(IEnumerable<string> columns, bool descending = false)
        {
            _orderbyColumns.AddRange(columns.Select(x => new KeyValuePair<string, bool>(x, descending)));
            return _client;
        }

        public IClientWithCommand OrderBy(params string[] columns)
        {
            return OrderBy(columns, false);
        }

        public IClientWithCommand OrderByDescending(IEnumerable<string> columns)
        {
            return OrderBy(columns, true);
        }

        public IClientWithCommand OrderByDescending(params string[] columns)
        {
            return OrderBy(columns, true);
        }

        public IClientWithCommand NavigateTo(string collectionName)
        {
            _navigateTo = _client.Chain(this);
            _navigateTo.Collection(collectionName);
            return _navigateTo;
        }

        public override string ToString()
        {
            return Format();
        }

        private string Format()
        {
            string commandText = string.Empty;
            if (_parent != null)
                commandText += _parent.ToString() + "/";

            commandText += _collectionName;
            var extraClauses = new List<string>();

            if (_key != null && _key.Count > 0 && !string.IsNullOrEmpty(_filter))
                throw new InvalidOperationException("Filter may not be set when key is assigned");

            if (_navigateTo != null && !string.IsNullOrEmpty(_filter))
                throw new InvalidOperationException("Filter may not be set for link navigations");

            if (_key != null && _key.Count > 0)
                commandText += "(" + (_key.Count == 1 ? FormatKeyItem(_key.First().Value) : string.Join(",", _key.Select(FormatKeyItem))) + ")";

            if (!string.IsNullOrEmpty(_filter))
                extraClauses.Add("$filter=" + HttpUtility.UrlEncode(_filter));

            if (_skipCount >= 0)
                extraClauses.Add("$skip=" + _skipCount.ToString());

            if (_topCount >= 0)
                extraClauses.Add("$top=" + _topCount.ToString());

            if (_expandAssociations.Any())
                extraClauses.Add("$expand=" + string.Join(",", _expandAssociations));

            if (_orderbyColumns.Any())
                extraClauses.Add("$orderby=" + string.Join(",", _orderbyColumns.Select(FormatOrderByItem)));

            if (_selectColumns.Any())
                extraClauses.Add("$select=" + string.Join(",", _selectColumns.Select(FormatSelectItem)));

            if (extraClauses.Any())
                commandText += "?" + string.Join("&", extraClauses);

            return commandText;
        }

        private string FormatKeyItem(object item)
        {
            // TODO
            return item.ToString();
        }

        private string FormatKeyItem(KeyValuePair<string, object> item)
        {
            // TODO
            return string.Format("{0}={1}", item.Key, item.Value);
        }

        private string FormatSelectItem(string item)
        {
            // TODO
            return item;
        }

        private string FormatOrderByItem(KeyValuePair<string,bool> item)
        {
            // TODO
            return item.Key + (item.Value ? " desc" : string.Empty);
        }
    }
}
