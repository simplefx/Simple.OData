using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    // ALthough FluentCommand is never instantiated directly (only via ICommand interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason FluentClient is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public class FluentCommand
    {
        private readonly Session _session;
        private readonly FluentCommand _parent;
        private string _collectionName;
        private ODataExpression _collectionExpression;
        private string _derivedCollectionName;
        private ODataExpression _derivedCollectionExpression;
        private string _functionName;
        private IList<object> _keyValues;
        private IDictionary<string, object> _namedKeyValues;
        private IDictionary<string, object> _entryData;
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private string _filter;
        private ODataExpression _filterExpression;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private readonly List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        private bool _computeCount;
        private bool _inlineCount;
        private string _linkName;
        private ODataExpression _linkExpression;

        internal static readonly string ResultLiteral = "__result";
        internal static readonly string ResourceTypeLiteral = "__resourcetype";

        internal FluentCommand(Session session, FluentCommand parent)
        {
            _session = session;
            _parent = parent;
        }

        internal FluentCommand(FluentCommand ancestor)
        {
            _session = ancestor._session;
            _parent = ancestor._parent;
            _collectionName = ancestor._collectionName;
            _collectionExpression = ancestor._collectionExpression;
            _derivedCollectionName = ancestor._derivedCollectionName;
            _derivedCollectionExpression = ancestor._derivedCollectionExpression;
            _functionName = ancestor._functionName;
            _keyValues = ancestor._keyValues;
            _namedKeyValues = ancestor._namedKeyValues;
            _entryData = ancestor._entryData;
            _parameters = ancestor._parameters;
            _filter = ancestor._filter;
            _filterExpression = ancestor._filterExpression;
            _filterExpression = ancestor._filterExpression;
            _skipCount = ancestor._skipCount;
            _topCount = ancestor._topCount;
            _expandAssociations = ancestor._expandAssociations;
            _selectColumns = ancestor._selectColumns;
            _orderbyColumns = ancestor._orderbyColumns;
            _computeCount = ancestor._computeCount;
            _inlineCount = ancestor._inlineCount;
            _linkName = ancestor._linkName;
            _linkExpression = ancestor._linkExpression;
        }

        private FluentCommand Resolve()
        {
            if (!ReferenceEquals(_collectionExpression, null))
            {
                For(_collectionExpression.AsString());
                _collectionExpression = null;
            }

            if (!ReferenceEquals(_derivedCollectionExpression, null))
            {
                As(_derivedCollectionExpression.AsString());
                _derivedCollectionExpression = null;
            }

            if (!ReferenceEquals(_filterExpression, null))
            {
                _namedKeyValues = TryInterpretFilterExpressionAsKey(_filterExpression);
                if (_namedKeyValues == null)
                {
                    _filter = _filterExpression.Format(_session, this.EntityCollection);
                }
                else
                {
                    _topCount = -1;
                }
                _filterExpression = null;
            }

            if (!ReferenceEquals(_linkExpression, null))
            {
                Link(_linkExpression.AsString());
                _linkExpression = null;
            }

            return this;
        }

        private EntityCollection EntityCollection
        {
            get
            {
                if (!string.IsNullOrEmpty(_collectionName))
                {
                    var entitySet = _session.Provider.GetMetadata().GetEntityCollection(_collectionName);
                    return string.IsNullOrEmpty(_derivedCollectionName)
                               ? entitySet
                               : _session.Provider.GetMetadata().GetDerivedEntityCollection(entitySet, _derivedCollectionName);
                }
                else if (!string.IsNullOrEmpty(_linkName))
                {
                    var parent = new FluentCommand(_parent).Resolve();
                    return _session.Provider.GetMetadata().GetEntityCollection(_session.Provider.GetMetadata()
                        .GetNavigationPropertyPartnerName(parent.EntityCollection.ActualName, _linkName));
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<string> GetCommandTextAsync()
        {
            await _session.ResolveProviderAsync(CancellationToken.None);
            return new FluentCommand(this).Resolve().Format();
        }

        public async Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            return new FluentCommand(this).Resolve().Format();
        }

        public void For(string collectionName)
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
        }

        public void For(ODataExpression expression)
        {
            _collectionExpression = expression;
        }

        public void As(string derivedCollectionName)
        {
            _derivedCollectionName = derivedCollectionName;
        }

        public void As(ODataExpression expression)
        {
            _derivedCollectionExpression = expression;
        }

        public void Link(string linkName)
        {
            _linkName = linkName;
        }

        public void Link(ODataExpression expression)
        {
            _linkExpression = expression;
        }

        public void Key(params object[] key)
        {
            if (key != null && key.Length == 1 && IsAnonymousType(key.First().GetType()))
            {
                var namedKeyValues = key.First();
                _namedKeyValues = Utils.GetMappedProperties(namedKeyValues.GetType())
                    .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(namedKeyValues, null))).ToIDictionary();
            }
            else
            {
                _keyValues = key.ToList();
            }
        }

        public void Key(IEnumerable<object> key)
        {
            _keyValues = key.ToList();
        }

        public void Key(IDictionary<string, object> key)
        {
            _namedKeyValues = key;
        }

        public void Filter(string filter)
        {
            _filter = filter;
        }

        public void Filter(ODataExpression expression)
        {
            _filterExpression = expression;
        }

        public void Skip(int count)
        {
            _skipCount = count;
        }

        public void Top(int count)
        {
            if (!HasKey)
            {
                _topCount = count;
            }
            else if (count != 1)
            {
                throw new InvalidOperationException("Top count may only be assigned to 1 when key is assigned");
            }
        }

        public void Expand(IEnumerable<string> associations)
        {
            _expandAssociations = associations.ToList();
        }

        public void Expand(params string[] associations)
        {
            _expandAssociations = associations.ToList();
        }

        public void Expand(params ODataExpression[] columns)
        {
            Expand(columns.Select(x => x.Reference));
        }

        public void Select(IEnumerable<string> columns)
        {
            _selectColumns = columns.ToList();
        }

        public void Select(params string[] columns)
        {
            _selectColumns = columns.ToList();
        }

        public void Select(params ODataExpression[] columns)
        {
            Select(columns.Select(x => x.Reference));
        }

        public void OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            _orderbyColumns.Clear();
            _orderbyColumns.AddRange(columns);
        }

        public void OrderBy(params string[] columns)
        {
            OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x, false)));
        }

        public void OrderBy(params ODataExpression[] columns)
        {
            OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x.Reference, false)));
        }

        public void ThenBy(params string[] columns)
        {
            _orderbyColumns.AddRange(columns.Select(x => new KeyValuePair<string, bool>(x, false)));
        }

        public void ThenBy(params ODataExpression[] columns)
        {
            ThenBy(columns.Select(x => x.Reference).ToArray());
        }

        public void OrderByDescending(params string[] columns)
        {
            OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x, true)));
        }

        public void OrderByDescending(params ODataExpression[] columns)
        {
            OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x.Reference, true)));
        }

        public void ThenByDescending(params string[] columns)
        {
            _orderbyColumns.AddRange(columns.Select(x => new KeyValuePair<string, bool>(x, true)));
        }

        public void ThenByDescending(params ODataExpression[] columns)
        {
            ThenByDescending(columns.Select(x => x.Reference).ToArray());
        }

        public void Count()
        {
            _computeCount = true;
        }

        public void Set(object value)
        {
            _entryData = Utils.GetMappedProperties(value.GetType())
                .Select(x => new KeyValuePair<string, object>(x.GetMappedName(), x.GetValue(value, null)))
                .ToDictionary();
        }

        public void Set(IDictionary<string, object> value)
        {
            _entryData = value;
        }

        public void Set(params ODataExpression[] value)
        {
            _entryData = value.Select(x => new KeyValuePair<string, object>(x.Reference, x.Value)).ToIDictionary();
        }

        public void Function(string functionName)
        {
            _functionName = functionName;
        }

        public void Parameters(IDictionary<string, object> parameters)
        {
            _parameters = parameters.ToDictionary();
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

        public FluentCommand WithInlineCount()
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
            get { return new FluentCommand(this).Resolve()._collectionName; }
        }

        internal bool HasKey
        {
            get { return _keyValues != null && _keyValues.Count > 0 || _namedKeyValues != null && _namedKeyValues.Count > 0; }
        }

        internal bool HasFilter
        {
            get { return !string.IsNullOrEmpty(_filter) || !ReferenceEquals(_filterExpression, null); }
        }

        internal IDictionary<string, object> KeyValues
        {
            get
            {
                if (!HasKey)
                    return null;

                var keyNames = _session.Provider.GetMetadata().GetDeclaredKeyPropertyNames(this.EntityCollection.ActualName).ToList();
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
                        var value = keyValue is ODataExpression ?
                            (keyValue as ODataExpression).Value :
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

        private string Format()
        {
            string commandText = string.Empty;
            if (!string.IsNullOrEmpty(_collectionName))
            {
                var entitySetName = _session.Provider.GetMetadata().GetEntitySetExactName(_collectionName);
                var entityTypeNamespace = _session.Provider.GetMetadata().GetEntitySetTypeNamespace(_collectionName);
                commandText += entitySetName;
                if (!string.IsNullOrEmpty(_derivedCollectionName))
                    commandText += "/" + string.Join(".",
                        entityTypeNamespace,
                        _session.Provider.GetMetadata().GetEntityTypeExactName(_derivedCollectionName));
            }
            else if (!string.IsNullOrEmpty(_linkName))
            {
                var parent = new FluentCommand(_parent).Resolve();
                commandText += parent.Format() + "/";
                commandText += _session.Provider.GetMetadata().GetNavigationPropertyExactName(parent.EntityCollection.ActualName, _linkName);
            }
            else if (!string.IsNullOrEmpty(_functionName))
            {
                commandText += _session.Provider.GetMetadata().GetFunctionExactName(_functionName);
            }

            if (HasKey && HasFilter)
                throw new InvalidOperationException("Filter may not be set when key is assigned");

            if (HasKey)
                commandText += FormatKey();

            commandText += FormatClauses();

            return commandText;
        }

        private string FormatClauses()
        {
            var text = string.Empty;
            var extraClauses = new List<string>();
            var aggregateClauses = new List<string>();

            if (_parameters.Any())
                extraClauses.Add(new ValueFormatter().Format(_parameters, "&"));

            if (_filter != null)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Filter, Uri.EscapeDataString(_filter)));

            if (_skipCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Skip, _skipCount));

            if (_topCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Top, _topCount));

            if (_expandAssociations.Any())
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Expand, string.Join(",", _expandAssociations.Select(FormatExpandItem))));

            if (_orderbyColumns.Any())
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.OrderBy, string.Join(",", _orderbyColumns.Select(FormatOrderByItem))));

            if (_selectColumns.Any())
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Select, string.Join(",", _selectColumns.Select(FormatSelectItem))));

            if (_inlineCount)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.InlineCount, ODataLiteral.AllPages));

            if (_computeCount)
                aggregateClauses.Add(ODataLiteral.Count);

            if (aggregateClauses.Any())
                text += "/" + string.Join("/", aggregateClauses);

            if (extraClauses.Any())
                text += "?" + string.Join("&", extraClauses);

            return text;
        }

        private string FormatExpandItem(string item)
        {
            var names = new List<string>();
            var items = item.Split('/');
            var entitySet = this.EntityCollection;
            foreach (var associationName in items)
            {
                names.Add(_session.Provider.GetMetadata().GetNavigationPropertyExactName(entitySet.ActualName, associationName));
                entitySet = _session.Provider.GetMetadata().GetEntityCollection(
                    _session.Provider.GetMetadata().GetNavigationPropertyPartnerName(entitySet.ActualName, associationName));
            }
            return string.Join("/", names);
        }

        private string FormatSelectItem(string item)
        {
            return _session.Provider.GetMetadata().HasStructuralProperty(this.EntityCollection.ActualName, item)
                ? _session.Provider.GetMetadata().GetStructuralPropertyExactName(this.EntityCollection.ActualName, item)
                : _session.Provider.GetMetadata().GetNavigationPropertyExactName(this.EntityCollection.ActualName, item);
        }

        private string FormatOrderByItem(KeyValuePair<string, bool> item)
        {
            return _session.Provider.GetMetadata().GetStructuralPropertyExactName(
                this.EntityCollection.ActualName, item.Key) + (item.Value ? " desc" : string.Empty);
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

        private IDictionary<string, object> TryInterpretFilterExpressionAsKey(ODataExpression expression)
        {
            bool ok = false;
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (!ReferenceEquals(expression, null))
            {
                ok = expression.ExtractEqualityComparisons(namedKeyValues);
            }
            if (!ok)
                return null;

            var keyNames = _session.Provider.GetMetadata().GetDeclaredKeyPropertyNames(this.EntityCollection.ActualName).ToList();
            return keyNames.Count == namedKeyValues.Count() && keyNames.All(namedKeyValues.ContainsKey) 
                ? namedKeyValues 
                : null;
        }

        private static bool IsAnonymousType(Type type)
        {
            // HACK: The only way to detect anonymous types right now.
            return type.HasCustomAttribute(typeof(CompilerGeneratedAttribute), false)
                       && type.IsGeneric() && type.Name.Contains("AnonymousType")
                       && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) ||
                           type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                       && (type.GetTypeAttributes() & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
