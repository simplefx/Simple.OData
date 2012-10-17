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
        private string _functionName;
        private Table _table;
        private IDictionary<string, object> _key;
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private string _filter;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        private bool _computeCount;
        private bool _scalarResult;
        private bool _setTotalCount;
        private ODataClientWithCommand _navigateTo;
        private string _linkName;

        internal static readonly string MetadataLiteral = "$metadata";
        internal static readonly string FilterLiteral = "$filter";
        internal static readonly string SkipLiteral = "$skip";
        internal static readonly string TopLiteral = "$top";
        internal static readonly string ExpandLiteral = "$expand";
        internal static readonly string OrderByLiteral = "$orderby";
        internal static readonly string SelectLiteral = "$select";
        internal static readonly string CountLiteral = "$count";
        internal static readonly string BatchLiteral = "$batch";
        internal static readonly string ResultLiteral = "$result";

        public ODataCommand(ODataClientWithCommand client, ODataCommand parent)
        {
            _client = client;
            _parent = parent;
        }

        public IClientWithCommand From(string collectionName)
        {
            _collectionName = collectionName;
            _table = _client.Schema.FindTable(_collectionName);
            return _client;
        }

        public IClientWithCommand Link(string linkName)
        {
            _linkName = linkName;
            _table = _client.Schema.FindTable(_parent._table.FindAssociation(_linkName).ReferenceTableName);
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

        public IClientWithCommand Count()
        {
            _computeCount = true;
            _scalarResult = true;
            return _client;
        }

        public IClientWithCommand Function(string functionName)
        {
            _functionName = functionName;
            return _client;
        }

        public IClientWithCommand Parameters(IDictionary<string, object> parameters)
        {
            _parameters = parameters.ToDictionary();
            return _client;
        }

        public IClientWithCommand NavigateTo(string linkName)
        {
            return _client.Link(this, linkName);
        }

        public override string ToString()
        {
            return Format();
        }

        private string Format()
        {
            string commandText = string.Empty;
            if (!string.IsNullOrEmpty(_collectionName))
            {
                commandText += _client.Schema.FindTable(_collectionName).ActualName;
            }
            else if (!string.IsNullOrEmpty(_linkName))
            {
                commandText += _parent.ToString() + "/";
                commandText += _parent._table.FindAssociation(_linkName).ActualName;
            }
            else if (!string.IsNullOrEmpty(_functionName))
            {
                commandText += _client.Schema.FindFunction(_functionName).ActualName;
            }

            var extraClauses = new List<string>();
            var aggregateClauses = new List<string>();

            if (_key != null && _key.Count > 0 && !string.IsNullOrEmpty(_filter))
                throw new InvalidOperationException("Filter may not be set when key is assigned");

            if (_navigateTo != null && !string.IsNullOrEmpty(_filter))
                throw new InvalidOperationException("Filter may not be set for link navigations");

            if (_key != null && _key.Count > 0)
                commandText += FormatKey();

            if (_parameters.Any())
                extraClauses.Add(new ValueFormatter().Format(_parameters, "&"));

            if (!string.IsNullOrEmpty(_filter))
                extraClauses.Add(string.Format("{0}={1}", FilterLiteral, HttpUtility.UrlEncode(_filter)));

            if (_skipCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", SkipLiteral, _skipCount));

            if (_topCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", TopLiteral, _topCount));

            if (_expandAssociations.Any())
                extraClauses.Add(string.Format("{0}={1}", ExpandLiteral, string.Join(",", _expandAssociations.Select(FormatExpandItem))));

            if (_orderbyColumns.Any())
                extraClauses.Add(string.Format("{0}={1}", OrderByLiteral, string.Join(",", _orderbyColumns.Select(FormatOrderByItem))));

            if (_selectColumns.Any())
                extraClauses.Add(string.Format("{0}={1}", SelectLiteral, string.Join(",", _selectColumns.Select(FormatSelectItem))));

            if (_computeCount)
                aggregateClauses.Add(CountLiteral);

            if (extraClauses.Any())
                commandText += "?" + string.Join("&", extraClauses);
            if (aggregateClauses.Any())
                commandText += "/" + string.Join("/", aggregateClauses);

            return commandText;
        }

        private string FormatExpandItem(string item)
        {
            return _table.FindAssociation(item).ActualName;
        }

        private string FormatSelectItem(string item)
        {
            return _table.HasColumn(item)
                ? _table.FindColumn(item).ActualName
                : _table.FindAssociation(item).ActualName;
        }

        private string FormatOrderByItem(KeyValuePair<string, bool> item)
        {
            return _table.FindColumn(item.Key) + (item.Value ? " desc" : string.Empty);
        }

        private string FormatKey()
        {
            var keyNames = _table.GetKeyNames();
            var namedKeyValues = new Dictionary<string, object>();
            foreach (var keyName in keyNames)
            {
                object keyValue;
                if (_key.TryGetValue(keyName, out keyValue))
                {
                    namedKeyValues.Add(keyName, keyValue);
                }
            }
            var valueFormatter = new ValueFormatter();
            var formattedKeyValues = namedKeyValues.Count == 1 ?
                valueFormatter.Format(namedKeyValues.Values) :
                valueFormatter.Format(namedKeyValues);
            return "(" + formattedKeyValues + ")";
        }
    }
}
