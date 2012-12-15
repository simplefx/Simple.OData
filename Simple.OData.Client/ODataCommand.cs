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
        private IList<object> _keyValues;
        private IDictionary<string, object> _namedKeyValues;
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private string _filter;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        private bool _computeCount;
        private bool _inlineCount;
        private string _linkName;

        internal static readonly string MetadataLiteral = "$metadata";
        internal static readonly string FilterLiteral = "$filter";
        internal static readonly string SkipLiteral = "$skip";
        internal static readonly string TopLiteral = "$top";
        internal static readonly string ExpandLiteral = "$expand";
        internal static readonly string OrderByLiteral = "$orderby";
        internal static readonly string SelectLiteral = "$select";
        internal static readonly string CountLiteral = "$count";
        internal static readonly string InlineCountLiteral = "$inlinecount";
        internal static readonly string AllPagesLiteral = "allpages";
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

        public IClientWithCommand Key(params object[] key)
        {
            _keyValues = key.ToList();
            return _client;
        }

        public IClientWithCommand Key(IEnumerable<object> key)
        {
            _keyValues = key.ToList();
            return _client;
        }

        public IClientWithCommand Key(IDictionary<string, object> key)
        {
            _namedKeyValues = key;
            return _client;
        }

        public IClientWithCommand Filter(string filter)
        {
            _filter = filter;
            return _client;
        }

        public IClientWithCommand Filter(FilterExpression expression)
        {
            _namedKeyValues = TryInterpretFilterExpressionAsKey(expression);
            if (_namedKeyValues == null)
            {
                _filter = expression.Format(_client, _table);
            }
            else
            {
                _topCount = -1;
            }
            return _client;
        }

        public IClientWithCommand Skip(int count)
        {
            _skipCount = count;
            return _client;
        }

        public IClientWithCommand Top(int count)
        {
            if (!HasKey)
            {
                _topCount = count;
            }
            else if (count != 1)
            {
                throw new InvalidOperationException("Top count may only be assigned to 1 when key is assigned");
            }
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

        public IClientWithCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            _orderbyColumns.AddRange(columns);
            return _client;
        }

        public IClientWithCommand OrderBy(params string[] columns)
        {
            return OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x, false)));
        }

        public IClientWithCommand OrderByDescending(params string[] columns)
        {
            return OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x, true)));
        }

        public IClientWithCommand Count()
        {
            _computeCount = true;
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

        public ODataCommand WithInlineCount()
        {
            _inlineCount = true;
            return this;
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

            if (_namedKeyValues != null && _namedKeyValues.Count > 0 && !string.IsNullOrEmpty(_filter))
                throw new InvalidOperationException("Filter may not be set when key is assigned");

            if (HasKey)
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

            if (_inlineCount)
                extraClauses.Add(string.Format("{0}={1}", InlineCountLiteral, AllPagesLiteral));

            if (_computeCount)
                aggregateClauses.Add(CountLiteral);

            if (aggregateClauses.Any())
                commandText += "/" + string.Join("/", aggregateClauses);

            if (extraClauses.Any())
                commandText += "?" + string.Join("&", extraClauses);

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
            for (int index = 0; index < keyNames.Count; index++)
            {
                if (_namedKeyValues != null && _namedKeyValues.Count > 0)
                {
                    object keyValue;
                    if (_namedKeyValues.TryGetValue(keyNames[index], out keyValue))
                    {
                        namedKeyValues.Add(keyNames[index], keyValue);
                    }
                }
                else if (_keyValues != null && _keyValues.Count >= index)
                {
                    namedKeyValues.Add(keyNames[index], _keyValues[index]);
                }
            }
            var valueFormatter = new ValueFormatter();
            var formattedKeyValues = namedKeyValues.Count == 1 ?
                valueFormatter.Format(namedKeyValues.Values) :
                valueFormatter.Format(namedKeyValues);
            return "(" + formattedKeyValues + ")";
        }

        private bool HasKey
        {
            get { return _keyValues != null && _keyValues.Count > 0 || _namedKeyValues != null && _namedKeyValues.Count > 0; }
        }

        private IDictionary<string, object> TryInterpretFilterExpressionAsKey(FilterExpression expression)
        {
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (!ReferenceEquals(expression, null))
            {
                expression.ExtractEqualityComparisons(namedKeyValues);
            }
            return _table.GetKeyNames().All(namedKeyValues.ContainsKey) ? namedKeyValues : null;
        }
    }
}
