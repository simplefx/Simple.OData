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

    public partial class FluentCommand
    {
        private readonly Session _session;
        private readonly FluentCommand _parent;
        private string _collectionName;
        private ODataExpression _collectionExpression;
        private string _derivedCollectionName;
        private ODataExpression _derivedCollectionExpression;
        private string _functionName;
        private string _actionName;
        private IList<object> _keyValues;
        private IDictionary<string, object> _namedKeyValues;
        private IDictionary<string, object> _entryData;
        private string _filter;
        private ODataExpression _filterExpression;
        private int _skipCount = -1;
        private int _topCount = -1;
        private List<string> _expandAssociations = new List<string>();
        private List<string> _selectColumns = new List<string>();
        private readonly List<KeyValuePair<string, bool>> _orderbyColumns = new List<KeyValuePair<string, bool>>();
        private bool _computeCount;
        private bool _includeCount;
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
            _actionName = ancestor._actionName;
            _keyValues = ancestor._keyValues;
            _namedKeyValues = ancestor._namedKeyValues;
            _entryData = ancestor._entryData;
            _filter = ancestor._filter;
            _filterExpression = ancestor._filterExpression;
            _filterExpression = ancestor._filterExpression;
            _skipCount = ancestor._skipCount;
            _topCount = ancestor._topCount;
            _expandAssociations = ancestor._expandAssociations;
            _selectColumns = ancestor._selectColumns;
            _orderbyColumns = ancestor._orderbyColumns;
            _computeCount = ancestor._computeCount;
            _includeCount = ancestor._includeCount;
            _linkName = ancestor._linkName;
            _linkExpression = ancestor._linkExpression;
        }

        private bool IsBatchResponse { get { return _session == null; } }

        private FluentCommand Resolve()
        {
            if (!ReferenceEquals(_collectionExpression, null))
            {
                For(_collectionExpression.AsString(_session));
                _collectionExpression = null;
            }

            if (!ReferenceEquals(_derivedCollectionExpression, null))
            {
                As(_derivedCollectionExpression.AsString(_session));
                _derivedCollectionExpression = null;
            }

            if (!ReferenceEquals(_filterExpression, null))
            {
                _namedKeyValues = TryInterpretFilterExpressionAsKey(_filterExpression);
                if (_namedKeyValues == null)
                {
                    _filter = _filterExpression.Format(new ExpressionContext(_session, this.EntityCollection));
                }
                else
                {
                    _keyValues = null;
                    _topCount = -1;
                }
                _filterExpression = null;
            }

            if (!ReferenceEquals(_linkExpression, null))
            {
                Link(_linkExpression.AsString(_session));
                _linkExpression = null;
            }

            return this;
        }

        private EntityCollection EntityCollection
        {
            get
            {
                if (string.IsNullOrEmpty(_collectionName) && string.IsNullOrEmpty(_linkName))
                    return null;

                EntityCollection entityCollection;
                if (!string.IsNullOrEmpty(_linkName))
                {
                    var parent = new FluentCommand(_parent).Resolve();
                    var collectionName = _session.Metadata.GetNavigationPropertyPartnerName(
                        parent.EntityCollection.Name, _linkName);
                    entityCollection = _session.Metadata.GetEntityCollection(collectionName);
                }
                else
                {
                    entityCollection = _session.Metadata.GetEntityCollection(_collectionName);
                }

                return string.IsNullOrEmpty(_derivedCollectionName)
                    ? entityCollection
                    : _session.Metadata.GetDerivedEntityCollection(entityCollection, _derivedCollectionName);
            }
        }

        public string QualifiedEntityCollectionName
        {
            get
            {
                var entityCollection = new FluentCommand(this).Resolve().EntityCollection;
                return entityCollection.BaseEntityCollection == null
                    ? entityCollection.Name
                    : string.Format("{0}/{1}.{2}",
                        entityCollection.BaseEntityCollection.Name,
                        _session.Metadata.GetEntityCollectionTypeNamespace(entityCollection.Name),
                        _session.Metadata.GetEntityCollectionTypeName(entityCollection.Name));
            }
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            return new FluentCommand(this).Resolve().Format();
        }

        public FluentCommand For(string collectionName)
        {
            if (IsBatchResponse) return this;

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
            return this;
        }

        public FluentCommand For(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            _collectionExpression = expression;
            return this;
        }

        public FluentCommand As(string derivedCollectionName)
        {
            if (IsBatchResponse) return this;

            _derivedCollectionName = derivedCollectionName;
            return this;
        }

        public FluentCommand As(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            _derivedCollectionExpression = expression;
            return this;
        }

        public FluentCommand Link(string linkName)
        {
            if (IsBatchResponse) return this;

            _linkName = linkName;
            return this;
        }

        public FluentCommand Link(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            _linkExpression = expression;
            return this;
        }

        public FluentCommand Key(params object[] key)
        {
            if (IsBatchResponse) return this;

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
            return this;
        }

        public FluentCommand Key(IEnumerable<object> key)
        {
            if (IsBatchResponse) return this;

            _keyValues = key.ToList();
            _namedKeyValues = null;
            return this;
        }

        public FluentCommand Key(IDictionary<string, object> key)
        {
            if (IsBatchResponse) return this;

            _namedKeyValues = key;
            _keyValues = null;
            return this;
        }

        public FluentCommand Filter(string filter)
        {
            if (IsBatchResponse) return this;

            _filter = filter;
            return this;
        }

        public FluentCommand Filter(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            _filterExpression = expression;
            return this;
        }

        public FluentCommand Skip(int count)
        {
            if (IsBatchResponse) return this;

            _skipCount = count;
            return this;
        }

        public FluentCommand Top(int count)
        {
            if (IsBatchResponse) return this;

            if (!HasKey)
            {
                _topCount = count;
            }
            else if (count != 1)
            {
                throw new InvalidOperationException("Top count may only be assigned to 1 when key is assigned");
            }
            return this;
        }

        public FluentCommand Expand(IEnumerable<string> associations)
        {
            if (IsBatchResponse) return this;

            _expandAssociations = SplitItems(associations).ToList();
            return this;
        }

        public FluentCommand Expand(params string[] associations)
        {
            if (IsBatchResponse) return this;

            _expandAssociations = SplitItems(associations).ToList();
            return this;
        }

        public FluentCommand Expand(params ODataExpression[] columns)
        {
            return Expand(columns.Select(x => x.Reference));
        }

        public FluentCommand Select(IEnumerable<string> columns)
        {
            if (IsBatchResponse) return this;

            _selectColumns = SplitItems(columns).ToList();
            return this;
        }

        public FluentCommand Select(params string[] columns)
        {
            if (IsBatchResponse) return this;

            _selectColumns = SplitItems(columns).ToList();
            return this;
        }

        public FluentCommand Select(params ODataExpression[] columns)
        {
            return Select(columns.Select(x => x.Reference));
        }

        public FluentCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            if (IsBatchResponse) return this;

            _orderbyColumns.Clear();
            _orderbyColumns.AddRange(SplitItems(columns));
            return this;
        }

        public FluentCommand OrderBy(params string[] columns)
        {
            return OrderBy(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, false)));
        }

        public FluentCommand OrderBy(params ODataExpression[] columns)
        {
            return OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x.Reference, false)));
        }

        public FluentCommand ThenBy(params string[] columns)
        {
            if (IsBatchResponse) return this;

            _orderbyColumns.AddRange(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public FluentCommand ThenBy(params ODataExpression[] columns)
        {
            return ThenBy(columns.Select(x => x.Reference).ToArray());
        }

        public FluentCommand OrderByDescending(params string[] columns)
        {
            return OrderBy(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, true)));
        }

        public FluentCommand OrderByDescending(params ODataExpression[] columns)
        {
            return OrderBy(columns.Select(x => new KeyValuePair<string, bool>(x.Reference, true)));
        }

        public FluentCommand ThenByDescending(params string[] columns)
        {
            if (IsBatchResponse) return this;

            _orderbyColumns.AddRange(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public FluentCommand ThenByDescending(params ODataExpression[] columns)
        {
            return ThenByDescending(columns.Select(x => x.Reference).ToArray());
        }

        public FluentCommand Count()
        {
            if (IsBatchResponse) return this;

            _computeCount = true;
            return this;
        }

        public FluentCommand Set(object value)
        {
            if (IsBatchResponse) return this;

            _entryData = Utils.GetMappedProperties(value.GetType())
                .Select(x => new KeyValuePair<string, object>(x.GetMappedName(), x.GetValue(value, null)))
                .ToDictionary();
            _session.EntryMap.GetOrAdd(value, _entryData);
            return this;
        }

        public FluentCommand Set(IDictionary<string, object> value)
        {
            if (IsBatchResponse) return this;

            _entryData = value;
            return this;
        }

        public FluentCommand Set(params ODataExpression[] value)
        {
            if (IsBatchResponse) return this;

            _entryData = value.Select(x => new KeyValuePair<string, object>(x.Reference, x.Value)).ToIDictionary();
            _session.EntryMap.GetOrAdd(value, _entryData);
            return this;
        }

        public FluentCommand Function(string functionName)
        {
            if (IsBatchResponse) return this;

            _functionName = functionName;
            return this;
        }

        public FluentCommand Action(string actionName)
        {
            if (IsBatchResponse) return this;

            _actionName = actionName;
            return this;
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

        public FluentCommand WithCount()
        {
            if (IsBatchResponse) return this;

            _includeCount = true;
            return this;
        }

        public override string ToString()
        {
            return Format();
        }

        internal bool HasKey
        {
            get { return _keyValues != null && _keyValues.Count > 0 || _namedKeyValues != null && _namedKeyValues.Count > 0; }
        }

        internal bool HasFilter
        {
            get { return !string.IsNullOrEmpty(_filter) || !ReferenceEquals(_filterExpression, null); }
        }

        public bool HasFunction
        {
            get
            {
                return !string.IsNullOrEmpty(_functionName);
            }
        }

        public bool HasAction
        {
            get
            {
                return !string.IsNullOrEmpty(_actionName);
            }
        }

        internal IDictionary<string, object> KeyValues
        {
            get
            {
                if (!HasKey)
                    return null;

                var keyNames = _session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();
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

        internal IDictionary<string, object> CommandData
        {
            get { return _entryData ?? new Dictionary<string, object>(); }
        }

        internal IList<string> SelectedColumns
        {
            get { return _selectColumns; }
        }

        internal string FunctionName
        {
            get { return _functionName; }
        }

        internal string ActionName
        {
            get { return _actionName; }
        }

        private string Format()
        {
            if (HasKey && HasFilter)
                throw new InvalidOperationException("OData filter and key may not be combined.");

            if (HasFunction && HasAction)
                throw new InvalidOperationException("OData function and action may not be combined.");

            var commandText = string.Empty;
            if (!string.IsNullOrEmpty(_collectionName))
            {
                commandText += _session.Metadata.GetEntityCollectionExactName(_collectionName);
            }
            else if (!string.IsNullOrEmpty(_linkName))
            {
                var parent = new FluentCommand(_parent).Resolve();
                commandText += string.Format("{0}/{1}",
                    parent.Format(),
                    _session.Metadata.GetNavigationPropertyExactName(parent.EntityCollection.Name, _linkName));
            }

            if (HasKey)
                commandText += _session.Adapter.ConvertKeyValuesToUriLiteral(this.KeyValues, true);

            if (!string.IsNullOrEmpty(_functionName) || !string.IsNullOrEmpty(_actionName))
            {
                if (!string.IsNullOrEmpty(_collectionName) || !string.IsNullOrEmpty(_linkName))
                    commandText += "/";
                if (!string.IsNullOrEmpty(_functionName))
                    commandText += _session.Metadata.GetFunctionFullName(_functionName);
                else
                    commandText += _session.Metadata.GetActionFullName(_actionName);
            }

            if (!string.IsNullOrEmpty(_functionName) && _session.Adapter.FunctionFormat == FunctionFormat.Key)
                commandText += _session.Adapter.ConvertKeyValuesToUriLiteral(this.CommandData, false);

            if (!string.IsNullOrEmpty(_derivedCollectionName))
            {
                var entityTypeNamespace = _session.Metadata.GetEntityCollectionTypeNamespace(_derivedCollectionName);
                var entityTypeName = _session.Metadata.GetEntityTypeExactName(_derivedCollectionName);
                commandText += string.Format("/{0}.{1}", entityTypeNamespace, entityTypeName);
            }

            commandText += FormatClauses();

            return commandText;
        }

        private string FormatClauses()
        {
            var text = string.Empty;
            var extraClauses = new List<string>();
            var aggregateClauses = new List<string>();

            if (this.CommandData.Any() && !string.IsNullOrEmpty(_functionName) &&
                _session.Adapter.FunctionFormat == FunctionFormat.Query)
                extraClauses.Add(string.Join("&", this.CommandData.Select(x => string.Format("{0}={1}",
                    x.Key, _session.Adapter.ConvertValueToUriLiteral(x.Value)))));

            if (_filter != null)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Filter, Uri.EscapeDataString(_filter)));

            if (_skipCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Skip, _skipCount));

            if (_topCount >= 0)
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Top, _topCount));

            if (!HasAction)
            {
                switch (_session.Adapter.AdapterVersion)
                {
                    case AdapterVersion.V3:
                        FormatClause(extraClauses, _expandAssociations, ODataLiteral.Expand, FormatExpandItem);
                        FormatClause(extraClauses, _selectColumns, ODataLiteral.Select, FormatSelectItem);
                        FormatClause(extraClauses, _orderbyColumns, ODataLiteral.OrderBy, FormatOrderByItem);
                        break;

                    case AdapterVersion.V4:
                        FormatExpandSelectOrderBy(extraClauses, this.EntityCollection, _expandAssociations, _selectColumns, _orderbyColumns);
                        break;
                }

                if (_includeCount)
                {
                    if (_session.Adapter.AdapterVersion == AdapterVersion.V3)
                        extraClauses.Add(string.Format("{0}={1}", ODataLiteral.InlineCount, ODataLiteral.AllPages));
                    else
                        extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Count, ODataLiteral.True));
                }

                if (_computeCount)
                    aggregateClauses.Add(ODataLiteral.Count);

                if (aggregateClauses.Any())
                    text += "/" + string.Join("/", aggregateClauses);
            }

            if (extraClauses.Any())
                text += "?" + string.Join("&", extraClauses);

            return text;
        }

        private IEnumerable<string> SplitItems(IEnumerable<string> columns)
        {
            return columns.SelectMany(x => x.Split(',').Select(y => y.Trim()));
        }

        private IEnumerable<KeyValuePair<string, bool>> SplitItems(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            return columns.SelectMany(x => x.Key.Split(',').Select(y => new KeyValuePair<string, bool>(y.Trim(), x.Value)));
        }

        private void FormatClause<T>(IList<string> extraClauses, IList<T> items,
            string clauseLiteral, Func<T, EntityCollection, string> formatItem)
        {
            if (items.Any())
            {
                extraClauses.Add(string.Format("{0}={1}", clauseLiteral,
                    string.Join(",", items.Select(x => formatItem(x, this.EntityCollection)))));
            }
        }

        private string FormatExpandItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var text = associationName;
            if (items.Count() == 1)
            {
                return text;
            }
            else
            {
                path = path.Substring(items.First().Length + 1);

                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                return string.Format("{0}/{1}", text, FormatExpandItem(path, entityCollection));
            }
        }

        private string FormatSelectItem(string path, EntityCollection entityCollection)
        {
            var items = path.Split('/');
            if (items.Count() == 1)
            {
                return _session.Metadata.HasStructuralProperty(entityCollection.Name, path)
                    ? _session.Metadata.GetStructuralPropertyExactName(entityCollection.Name, path)
                    : _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, path);
            }
            else
            {
                var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                path = path.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text, FormatSelectItem(path, entityCollection));
            }
        }

        private string FormatOrderByItem(KeyValuePair<string, bool> pathWithOrder, EntityCollection entityCollection)
        {
            var items = pathWithOrder.Key.Split('/');
            if (items.Count() == 1)
            {
                var clause = _session.Metadata.HasStructuralProperty(entityCollection.Name, pathWithOrder.Key)
                    ? _session.Metadata.GetStructuralPropertyExactName(entityCollection.Name, pathWithOrder.Key)
                    : _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, pathWithOrder.Key);
                if (pathWithOrder.Value)
                    clause += " desc";
                return clause;
            }
            else
            {
                var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());
                var text = associationName;
                var item = pathWithOrder.Key.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));
                return string.Format("{0}/{1}", text,
                    FormatOrderByItem(new KeyValuePair<string, bool>(item, pathWithOrder.Value), entityCollection));
            }
        }

        private void FormatExpandSelectOrderBy(IList<string> extraClauses, EntityCollection entityCollection,
            IList<string> expandAssociations, IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns)
        {
            if (expandAssociations.Any())
            {
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Expand,
                    string.Join(",", expandAssociations.Select(x =>
                        FormatExpandSelectOrderByItem(x, this.EntityCollection, selectColumns, orderbyColumns)))));
            }

            selectColumns = selectColumns.Where(x => !x.Contains("/")).ToList();
            if (selectColumns.Any())
            {
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.Select,
                    string.Join(",", selectColumns.Select(x => FormatSelectItem(x, this.EntityCollection)))));
            }

            orderbyColumns = orderbyColumns.Where(x => !x.Key.Contains("/")).ToList();
            if (orderbyColumns.Any())
            {
                extraClauses.Add(string.Format("{0}={1}", ODataLiteral.OrderBy,
                    string.Join(",", orderbyColumns.Select(x => FormatOrderByItem(x, this.EntityCollection)))));
            }
        }

        private string FormatExpandSelectOrderByItem(string path, EntityCollection entityCollection,
            IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns)
        {
            var items = path.Split('/');
            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var text = string.Empty;
            if (items.Count() == 1)
            {
                selectColumns = selectColumns
                        .Where(x => x.Contains("/") && x.Split('/').First() == associationName)
                        .Select(x => string.Join("/", x.Split('/').Skip(1))).ToList();
                orderbyColumns = orderbyColumns
                        .Where(x => x.Key.Contains("/") && x.Key.Split('/').First() == associationName)
                        .Select(x => new KeyValuePair<string, bool>(
                            string.Join("/", x.Key.Split('/').Skip(1)), x.Value)).ToList();

                if (selectColumns.Any())
                {
                    var columns = string.Join(",", selectColumns.Where(x => !x.Contains("/")).ToList());
                    text += string.Format("{0}({1}={2})", associationName, ODataLiteral.Select, columns);
                }
                if (orderbyColumns.Any())
                {
                    var columns = string.Join(",", orderbyColumns.Where(x => !x.Key.Contains("/"))
                        .Select(x => x.Key + (x.Value ? " desc" : string.Empty)).ToList());
                    if (!string.IsNullOrEmpty(text)) text += ",";
                    text += string.Format("{0}({1}={2})", associationName, ODataLiteral.OrderBy, columns);
                }
                return string.IsNullOrEmpty(text) ? associationName : text;
            }
            else
            {
                path = path.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                selectColumns = selectColumns
                        .Where(x => x.Contains("/") && x.Split('/').First() == items.First())
                        .Select(x => string.Join("/", x.Split('/').Skip(1))).ToList();
                orderbyColumns = orderbyColumns
                        .Where(x => x.Key.Contains("/") && x.Key.Split('/').First() == items.First())
                        .Select(x => new KeyValuePair<string, bool>(
                            string.Join("/", x.Key.Split('/').Skip(1)), x.Value)).ToList();

                if (selectColumns.Any())
                {
                    selectColumns = selectColumns.Where(x => !x.Contains("/")).ToList();
                    text += string.Format("{0}({1}={2})",
                        associationName, ODataLiteral.Expand, 
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns));
                }
                if (orderbyColumns.Any())
                {
                    orderbyColumns = orderbyColumns.Where(x => !x.Key.Contains("/")).ToList();
                    if (!string.IsNullOrEmpty(text)) text += ",";
                    text += string.Format("{0}({1}={2})",
                        associationName, ODataLiteral.Expand,
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns));
                }

                return string.IsNullOrEmpty(text)
                    ? string.Format("{0}({1}={2})", associationName, ODataLiteral.Expand,
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns))
                    : text;
            }
        }

        private IDictionary<string, object> TryInterpretFilterExpressionAsKey(ODataExpression expression)
        {
            bool ok = false;
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (!ReferenceEquals(expression, null))
            {
                ok = expression.ExtractLookupColumns(namedKeyValues);
            }
            if (!ok)
                return null;

            var keyNames = _session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();
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
