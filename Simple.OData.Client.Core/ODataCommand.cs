using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    class ODataCommand : ICommand
    {
        private readonly ODataClientWithCommand _client;
        private readonly ODataCommand _parent;
        private string _collectionName;
        private string _derivedCollectionName;
        private string _functionName;
        private IList<object> _keyValues;
        private IDictionary<string, object> _namedKeyValues;
        private IDictionary<string, object> _entryData;
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private string _filter;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private readonly List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
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
        internal static readonly string ResultLiteral = "__result";
        internal static readonly string ResourceTypeLiteral = "__resourcetype";

        public ODataCommand(ODataClientWithCommand client, ODataCommand parent)
        {
            _client = client;
            _parent = parent;
        }

        private Table Table
        {
            get
            {
                if (!string.IsNullOrEmpty(_collectionName))
                {
                    var table = _client.Schema.FindTable(_collectionName);
                    return string.IsNullOrEmpty(_derivedCollectionName)
                               ? table
                               : table.FindDerivedTable(_derivedCollectionName);
                }
                else if (!string.IsNullOrEmpty(_linkName))
                {
                    return _client.Schema.FindTable(_parent.Table.FindAssociation(_linkName).ReferenceTableName);
                }
                else
                {
                    return null;
                }
            }
        }

        public IClientWithCommand For(string collectionName)
        {
            var items = collectionName.Split('/');
            if (items.Count() > 1)
            {
                _collectionName = items[0];
                _derivedCollectionName = items[1];
            }
            else
            {
                _collectionName = collectionName;
            }
            return _client;
        }

        public IClientWithCommand As(string derivedCollectionName)
        {
            _derivedCollectionName = derivedCollectionName;
            return _client;
        }

        public IClientWithCommand Link(string linkName)
        {
            _linkName = linkName;
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
                _filter = expression.Format(_client, this.Table);
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

        public IClientWithCommand Set(object value)
        {
            var properties = value.GetType().GetProperties();
            var dict = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                dict.Add(property.Name, property.GetValue(value, null));
            }
            _entryData = dict;
            return _client;
        }

        public IClientWithCommand Set(IDictionary<string, object> value)
        {
            _entryData = value;
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

        public bool FilterIsKey
        {
            get
            {
                return _namedKeyValues != null;
            }
        }

        public IDictionary<string, object> FilterAsKey
        {
            get
            {
                return _namedKeyValues;
            }
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

        internal string CollectionName
        {
            get { return _collectionName; }
        }

        internal IDictionary<string, object> KeyValues
        {
            get
            {
                if (!HasKey)
                    return null;

                var keyNames = this.Table.GetKeyNames();
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
                return namedKeyValues;
            }
        }

        internal IDictionary<string, object> EntryData
        {
            get { return _entryData; }
        }

        private string Format()
        {
            string commandText = string.Empty;
            if (!string.IsNullOrEmpty(_collectionName))
            {
                commandText += _client.Schema.FindTable(_collectionName).ActualName;
                if (!string.IsNullOrEmpty(_derivedCollectionName))
                    commandText += "/" + string.Join(".", _client.Schema.TypesNamespace, _derivedCollectionName);
            }
            else if (!string.IsNullOrEmpty(_linkName))
            {
                commandText += _parent.ToString() + "/";
                commandText += _parent.Table.FindAssociation(_linkName).ActualName;
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
                extraClauses.Add(string.Format("{0}={1}", FilterLiteral, Uri.EscapeDataString(_filter)));

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
            return this.Table.FindAssociation(item).ActualName;
        }

        private string FormatSelectItem(string item)
        {
            return this.Table.HasColumn(item)
                ? this.Table.FindColumn(item).ActualName
                : this.Table.FindAssociation(item).ActualName;
        }

        private string FormatOrderByItem(KeyValuePair<string, bool> item)
        {
            return this.Table.FindColumn(item.Key) + (item.Value ? " desc" : string.Empty);
        }

        private string FormatKey()
        {
            var namedKeyValues = this.KeyValues;
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
            bool ok = false;
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (!ReferenceEquals(expression, null))
            {
                ok = expression.ExtractEqualityComparisons(namedKeyValues);
            }
            return ok && 
                this.Table.GetKeyNames().Count == namedKeyValues.Count() && 
                this.Table.GetKeyNames().All(namedKeyValues.ContainsKey) ? namedKeyValues : null;
        }
    }
}
