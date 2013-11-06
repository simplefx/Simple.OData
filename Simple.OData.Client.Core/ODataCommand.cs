using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    // ALthough ODataCommand is never instantiated directly (only via ICommand interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason ODataClientWithCommand is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public class ODataCommand : ICommand
    {
        protected ODataClientWithCommand _client;
        protected readonly ODataCommand _parent;
        protected string _collectionName;
        protected string _derivedCollectionName;
        protected string _functionName;
        protected IList<object> _keyValues;
        protected IDictionary<string, object> _namedKeyValues;
        protected IDictionary<string, object> _entryData;
        protected Dictionary<string, object> _parameters = new Dictionary<string, object>();
        protected string _filter;
        protected int _skipCount = -1;
        protected int _topCount = -1;
        protected List<string> _expandAssociations = new List<string>();
        protected List<string> _selectColumns = new List<string>();
        protected readonly List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        protected bool _computeCount;
        protected bool _inlineCount;
        protected string _linkName;

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

        internal ODataCommand(ODataCommand ancestor)
        {
            _client = ancestor._client;
            _parent = ancestor._parent;
            _collectionName = ancestor._collectionName;
            _derivedCollectionName = ancestor._derivedCollectionName;
            _functionName = ancestor._functionName;
            _keyValues = ancestor._keyValues;
            _namedKeyValues = ancestor._namedKeyValues;
            _entryData = ancestor._entryData;
            _parameters = ancestor._parameters;
            _filter = ancestor._filter;
            _skipCount = ancestor._skipCount;
            _topCount = ancestor._topCount;
            _expandAssociations = ancestor._expandAssociations;
            _selectColumns = ancestor._selectColumns;
            _orderbyColumns = ancestor._orderbyColumns;
            _computeCount = ancestor._computeCount;
            _inlineCount = ancestor._inlineCount;
            _linkName = ancestor._linkName;
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
            var properties = value.GetType().GetDeclaredProperties();
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
            get { return _client.Schema.FindTable(_collectionName).ActualName; }
        }

        internal bool HasKey
        {
            get { return _keyValues != null && _keyValues.Count > 0 || _namedKeyValues != null && _namedKeyValues.Count > 0; }
        }

        internal bool HasFilter
        {
            get { return !string.IsNullOrEmpty(_filter); }
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
                    bool found = false;
                    object keyValue = null;
                    if (_namedKeyValues != null && _namedKeyValues.Count > 0)
                    {
                        found = _namedKeyValues.TryGetValue(keyNames[index], out keyValue);
                    }
                    else if (_keyValues != null && _keyValues.Count >= index)
                    {
                        keyValue = _keyValues[index];
                        found = true;
                    }
                    if (found)
                    {
                        var value = keyValue is FilterExpression ? 
                            (keyValue as FilterExpression).Value : 
                            keyValue;
                        namedKeyValues.Add(keyNames[index], value);
                    }
                }
                return namedKeyValues;
            }
        }

        internal IDictionary<string, object> EntryData
        {
            get { return _entryData; }
        }

        internal IList<string> SelectedColumns
        {
            get { return _selectColumns; }
        }

        protected string Format()
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
                commandText += _parent + "/";
                commandText += _parent.Table.FindAssociation(_linkName).ActualName;
            }
            else if (!string.IsNullOrEmpty(_functionName))
            {
                commandText += _client.Schema.FindFunction(_functionName).ActualName;
            }

            if (HasKey && HasFilter)
                throw new InvalidOperationException("Filter may not be set when key is assigned");

            if (HasKey)
                commandText += FormatKey();

            commandText += FormatClauses();

            return commandText;
        }

        protected string FormatClauses()
        {
            var text = string.Empty;
            var extraClauses = new List<string>();
            var aggregateClauses = new List<string>();

            if (_parameters.Any())
                extraClauses.Add(new ValueFormatter().Format(_parameters, "&"));

            if (HasFilter)
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
                text += "/" + string.Join("/", aggregateClauses);

            if (extraClauses.Any())
                text += "?" + string.Join("&", extraClauses);

            return text;
        }

        protected string FormatExpandItem(string item)
        {
            return this.Table.FindAssociation(item).ActualName;
        }

        protected string FormatSelectItem(string item)
        {
            return this.Table.HasColumn(item)
                ? this.Table.FindColumn(item).ActualName
                : this.Table.FindAssociation(item).ActualName;
        }

        protected string FormatOrderByItem(KeyValuePair<string, bool> item)
        {
            return this.Table.FindColumn(item.Key) + (item.Value ? " desc" : string.Empty);
        }

        protected string FormatKey()
        {
            var namedKeyValues = this.KeyValues;
            var valueFormatter = new ValueFormatter();
            var formattedKeyValues = namedKeyValues.Count == 1 ?
                valueFormatter.Format(namedKeyValues.Values) :
                valueFormatter.Format(namedKeyValues);
            return "(" + formattedKeyValues + ")";
        }

        protected IDictionary<string, object> TryInterpretFilterExpressionAsKey(FilterExpression expression)
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

        protected IEnumerable<string> ExtractColumnNames<T>(Expression<Func<T, object>> expression)
        {
            var lambdaExpression = Utils.CastExpressionWithTypeCheck<LambdaExpression>(expression);
            switch (lambdaExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return new [] { (lambdaExpression.Body as MemberExpression).Member.Name };

                case ExpressionType.New:
                    var newExpression = lambdaExpression.Body as NewExpression;
                    if (newExpression.Arguments.Any(x => x.NodeType != ExpressionType.MemberAccess))
                        throw new NotSupportedException(string.Format("Not supported arguments in anonymous type creation expression {0}", expression));
                    return newExpression.Arguments.Select(x => (x as MemberExpression).Member.Name);

                default:
                    throw Utils.NotSupportedExpression(lambdaExpression.Body);
            }
        }
    }

    public class ODataCommand<T> : ODataCommand, ICommand<T>
    {
        public ODataCommand(ODataClientWithCommand<T> client, ODataCommand parent)
            : base(client, parent)
        {
        }

        internal ODataCommand(ODataCommand ancestor)
            : base(ancestor)
        {
        }

        private ODataClientWithCommand<T> CastClient
        {
            get
            {
                return _client as ODataClientWithCommand<T>;
            }
        }

        internal ODataClientWithCommand<T> Client
        {
            set { _client = value; }
        }

        public new IClientWithCommand<T> For(string collectionName = null)
        {
            base.For(collectionName ?? typeof(T).Name);
            return CastClient;
        }

        public IClientWithCommand<U> As<U>(string derivedCollectionName = null)
        {
            base.As(derivedCollectionName ?? typeof(U).Name);
            return new ODataClientWithCommand<U>(_client, this);
        }

        public IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression)
        {
            base.Filter(FilterExpression.FromLinqExpression(expression.Body));
            return CastClient;
        }

        public IClientWithCommand<T> Expand(Expression<Func<T, object>> expression)
        {
            base.Expand(ExtractColumnNames(expression));
            return CastClient;
        }

        public IClientWithCommand<T> Select(Expression<Func<T, object>> expression)
        {
            base.Select(ExtractColumnNames(expression));
            return CastClient;
        }

        public IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression)
        {
            base.Select(ExtractColumnNames(expression));
            return CastClient;
        }

        public IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return CastClient;
        }

        public IClientWithCommand<U> NavigateTo<U>(string linkName = null)
        {
            return CastClient.Link<U>(this, linkName);
        }
    }
}
