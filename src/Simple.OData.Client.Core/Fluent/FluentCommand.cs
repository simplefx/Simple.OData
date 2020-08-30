using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    // Although FluentCommand is never instantiated directly (only via ICommand interface)
    // it's declared as public in order to resolve problem when it is used with dynamic C#
    // For the same reason FluentClient is also declared as public
    // More: http://bloggingabout.net/blogs/vagif/archive/2013/08/05/we-need-better-interoperability-between-dynamic-and-statically-compiled-c.aspx

    public partial class FluentCommand
    {
        internal static readonly string ResultLiteral = "__result";
        internal static readonly string AnnotationsLiteral = "__annotations";
        internal static readonly string MediaEntityLiteral = "__entity";

        internal FluentCommand(Session session, FluentCommand parent, ConcurrentDictionary<object, IDictionary<string, object>> batchEntries)
        {
            Details = new CommandDetails(session, parent, batchEntries);
        }

        internal FluentCommand(FluentCommand ancestor)
        {
            Details = new CommandDetails(ancestor.Details);
        }

        internal CommandDetails Details { get; private set; }
        private bool IsBatchResponse => Details.Session == null;

        internal ITypeCache TypeCache => Details.Session.TypeCache;


        internal ResolvedCommand Resolve(ISession session)
        {
            return new ResolvedCommand(this, session as Session);
        }

        public string DynamicPropertiesContainerName => Details.DynamicPropertiesContainerName;

        public FluentCommand For(string collectionName)
        {
            if (IsBatchResponse) return this;

            var items = collectionName.Split('/');
            if (items.Count() > 1)
            {
                Details.CollectionName = items[0];
                Details.DerivedCollectionName = items[1];
            }
            else
            {
                Details.CollectionName = collectionName;
            }
            return this;
        }

        public FluentCommand WithProperties(string propertyName)
        {
            if (IsBatchResponse) return this;

            Details.DynamicPropertiesContainerName = propertyName;
            return this;
        }

        public FluentCommand WithMedia(IEnumerable<string> properties)
        {
            if (IsBatchResponse) return this;

            Details.MediaProperties = properties;
            return this;
        }

        public FluentCommand WithMedia(params string[] properties)
        {
            if (IsBatchResponse) return this;

            Details.MediaProperties = SplitItems(properties).ToList();
            return this;
        }

        public FluentCommand WithMedia(params ODataExpression[] properties)
        {
            return WithMedia(properties.Select(x => x.Reference));
        }

        public FluentCommand For(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            Details.CollectionExpression = expression;
            return this;
        }

        public FluentCommand As(string derivedCollectionName)
        {
            if (IsBatchResponse) return this;

            Details.DerivedCollectionName = derivedCollectionName;
            return this;
        }

        public FluentCommand As(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            Details.DerivedCollectionExpression = expression;
            return this;
        }

        public FluentCommand Link(string linkName)
        {
            if (IsBatchResponse) return this;

            Details.LinkName = linkName;
            return this;
        }

        public FluentCommand Link(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            Details.LinkExpression = expression;
            return this;
        }

        public FluentCommand Key(params object[] key)
        {
            if (IsBatchResponse) return this;

            if (key != null && key.Length == 1 && TypeCache.IsAnonymousType(key.First().GetType()))
            {
                return Key(TypeCache.ToDictionary(key.First()));
            }
            else
            {
                return Key(key.ToList());
            }
        }

        public FluentCommand Key(IEnumerable<object> key)
        {
            if (IsBatchResponse) return this;

            Details.KeyValues = key.ToList();
            Details.NamedKeyValues = null;
            Details.IsAlternateKey = false;
            return this;
        }

        public FluentCommand Key(IDictionary<string, object> key)
        {
            if (IsBatchResponse) return this;

            Details.KeyValues = null;
            Details.NamedKeyValues = key;
            Details.IsAlternateKey = false;
            return this;
        }

        public FluentCommand Filter(string filter)
        {
            if (IsBatchResponse) return this;

            if (string.IsNullOrEmpty(Details.Filter))
                Details.Filter = filter;
            else
                Details.Filter = $"({Details.Filter}) and ({filter})";
            return this;
        }

        public FluentCommand Filter(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            if (ReferenceEquals(Details.FilterExpression, null))
                Details.FilterExpression = expression;
            else
                Details.FilterExpression = Details.FilterExpression && expression;
            return this;
        }

        public FluentCommand Search(string search)
        {
            if (IsBatchResponse) return this;

            Details.Search = search;
            return this;
        }

        public FluentCommand Skip(long count)
        {
            if (IsBatchResponse) return this;

            Details.SkipCount = count;
            return this;
        }

        public FluentCommand Top(long count)
        {
            if (IsBatchResponse) return this;

            if (!HasKey || HasFunction)
            {
                Details.TopCount = count;
            }
            else if (count != 1)
            {
                throw new InvalidOperationException("Top count may only be assigned to 1 when key is assigned.");
            }
            return this;
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions)
        {
            if (IsBatchResponse) return this;

            Details.ExpandAssociations.AddRange(new[] { new KeyValuePair<string, ODataExpandOptions>("*", ODataExpandOptions.ByValue()) });
            return this;
        }

        public FluentCommand Expand(IEnumerable<string> associations)
        {
            if (IsBatchResponse) return this;

            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, ODataExpandOptions.ByValue())));
            return this;
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions, IEnumerable<string> associations)
        {
            if (IsBatchResponse) return this;

            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, expandOptions)));
            return this;
        }

        public FluentCommand Expand(params string[] associations)
        {
            if (IsBatchResponse) return this;

            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, ODataExpandOptions.ByValue())));
            return this;
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions, params string[] associations)
        {
            if (IsBatchResponse) return this;

            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, expandOptions)));
            return this;
        }

        public FluentCommand Expand(params ODataExpression[] columns)
        {
            return Expand(columns.Select(x => x.Reference));
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions, params ODataExpression[] columns)
        {
            return Expand(expandOptions, columns.Select(x => x.Reference));
        }

        public FluentCommand Select(IEnumerable<string> columns)
        {
            if (IsBatchResponse) return this;

            Details.SelectColumns.AddRange(SplitItems(columns).ToList());
            return this;
        }

        public FluentCommand Select(params string[] columns)
        {
            if (IsBatchResponse) return this;

            Details.SelectColumns.AddRange(SplitItems(columns).ToList());
            return this;
        }

        public FluentCommand Select(params ODataExpression[] columns)
        {
            return Select(columns.Select(x => x.Reference));
        }

        public FluentCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            if (IsBatchResponse) return this;

            Details.OrderbyColumns.AddRange(SplitItems(columns));
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

            Details.OrderbyColumns.AddRange(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, false)));
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

            Details.OrderbyColumns.AddRange(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public FluentCommand ThenByDescending(params ODataExpression[] columns)
        {
            return ThenByDescending(columns.Select(x => x.Reference).ToArray());
        }

        public FluentCommand QueryOptions(string queryOptions)
        {
            if (IsBatchResponse) return this;

            if (Details.QueryOptions == null)
                Details.QueryOptions = queryOptions;
            else
                Details.QueryOptions = $"{Details.QueryOptions}&{queryOptions}";
            return this;
        }

        public FluentCommand QueryOptions(IDictionary<string, object> queryOptions)
        {
            if (IsBatchResponse) return this;

            Details.QueryOptionsKeyValues = queryOptions;
            return this;
        }

        public FluentCommand QueryOptions(ODataExpression expression)
        {
            if (IsBatchResponse) return this;

            if (ReferenceEquals(Details.QueryOptionsExpression, null))
                Details.QueryOptionsExpression = expression;
            else
                Details.QueryOptionsExpression = Details.QueryOptionsExpression && expression;
            return this;
        }

        public FluentCommand Media()
        {
            return Media(FluentCommand.MediaEntityLiteral);
        }

        public FluentCommand Media(string streamName)
        {
            if (IsBatchResponse) return this;

            Details.MediaName = streamName;
            return this;
        }

        public FluentCommand Media(ODataExpression expression)
        {
            return Media(expression.Reference);
        }

        public FluentCommand Count()
        {
            if (IsBatchResponse) return this;

            Details.ComputeCount = true;
            return this;
        }

        public FluentCommand Set(object value)
        {
            if (IsBatchResponse) return this;

            Details.EntryData = TypeCache.ToDictionary(value);
            Details.BatchEntries?.GetOrAdd(value, Details.EntryData);

            return this;
        }

        public FluentCommand Set(IDictionary<string, object> value)
        {
            if (IsBatchResponse) return this;

            Details.EntryData = value;
            return this;
        }

        public FluentCommand Set(params ODataExpression[] value)
        {
            if (IsBatchResponse) return this;

            Details.EntryData = value.Select(x => new KeyValuePair<string, object>(x.Reference, x.Value)).ToIDictionary();
            Details.BatchEntries?.GetOrAdd(value, Details.EntryData);

            return this;
        }

        public FluentCommand Function(string functionName)
        {
            if (IsBatchResponse) return this;

            Details.FunctionName = functionName;
            return this;
        }

        public FluentCommand Action(string actionName)
        {
            if (IsBatchResponse) return this;

            Details.ActionName = actionName;
            return this;
        }

        public bool FilterIsKey => Details.NamedKeyValues != null;

        public IDictionary<string, object> FilterAsKey => Details.NamedKeyValues;

        public FluentCommand WithCount()
        {
            if (IsBatchResponse) return this;

            Details.IncludeCount = true;
            return this;
        }

        internal bool HasKey => Details.KeyValues != null && Details.KeyValues.Count > 0 || Details.NamedKeyValues != null && Details.NamedKeyValues.Count > 0;

        internal bool HasFilter => !string.IsNullOrEmpty(Details.Filter) || !ReferenceEquals(Details.FilterExpression, null);

        internal bool HasSearch => !string.IsNullOrEmpty(Details.Search);

        public bool HasFunction => !string.IsNullOrEmpty(Details.FunctionName);

        public bool HasAction => !string.IsNullOrEmpty(Details.ActionName);

        internal IDictionary<string, object> KeyValues => !HasKey ? null : Details.NamedKeyValues;

        internal IDictionary<string, object> CommandData
        {
            get
            {
                if (Details.EntryData == null)
                    return new Dictionary<string, object>();
                if (string.IsNullOrEmpty(Details.DynamicPropertiesContainerName))
                    return Details.EntryData;

                var entryData = new Dictionary<string, object>();
                foreach (var key in Details.EntryData.Keys.Where(x =>
                    !string.Equals(x, Details.DynamicPropertiesContainerName, StringComparison.OrdinalIgnoreCase)))
                {
                    entryData.Add(key, Details.EntryData[key]);
                }

                if (Details.EntryData.TryGetValue(Details.DynamicPropertiesContainerName, out var dynamicProperties) && dynamicProperties != null)
                {
                    if (dynamicProperties is IDictionary<string, object> kv)
                    {
                        foreach (var key in kv.Keys)
                        {
                            entryData.Add(key, kv[key]);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Property {Details.DynamicPropertiesContainerName} must implement IDictionary<string,object> interface");
                    }
                }

                return entryData;
            }
        }

        internal IList<string> SelectedColumns => Details.SelectColumns;

        internal string FunctionName => Details.FunctionName;

        internal string ActionName => Details.ActionName;

        private IEnumerable<string> SplitItems(IEnumerable<string> columns)
        {
            return columns.SelectMany(x => x.Split(',').Select(y => y.Trim()));
        }

        private IEnumerable<KeyValuePair<string, bool>> SplitItems(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            return columns.SelectMany(x => x.Key.Split(',').Select(y => new KeyValuePair<string, bool>(y.Trim(), x.Value)));
        }
    }
}
