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
            Session = session;
            Details = new FluentCommandDetails(parent, batchEntries);
        }

        internal FluentCommand(FluentCommand command)
        {
            Session = command.Session;
            Details = new FluentCommandDetails(command.Details);
        }

        internal FluentCommand(ResolvedCommand command)
        {
            Session = command.Session;
            Details = new FluentCommandDetails(command.Details);
        }

        internal Session Session { get; private set; }

        internal FluentCommandDetails Details { get; private set; }

        internal ITypeCache TypeCache => Session.TypeCache;


        internal ResolvedCommand Resolve(ISession session)
        {
            return new ResolvedCommand(this, session as Session);
        }

        public string DynamicPropertiesContainerName => Details.DynamicPropertiesContainerName;

        public FluentCommand For(string collectionName)
        {
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
            Details.DynamicPropertiesContainerName = propertyName;
            return this;
        }

        public FluentCommand WithMedia(IEnumerable<string> properties)
        {
            Details.MediaProperties = properties;
            return this;
        }

        public FluentCommand WithMedia(params string[] properties)
        {
            Details.MediaProperties = SplitItems(properties).ToList();
            return this;
        }

        public FluentCommand WithMedia(params ODataExpression[] properties)
        {
            return WithMedia(properties.Select(x => x.Reference));
        }

        public FluentCommand For(ODataExpression expression)
        {
            Details.CollectionExpression = expression;
            return this;
        }

        public FluentCommand As(string derivedCollectionName)
        {
            Details.DerivedCollectionName = derivedCollectionName;
            return this;
        }

        public FluentCommand As(ODataExpression expression)
        {
            Details.DerivedCollectionExpression = expression;
            return this;
        }

        public FluentCommand Link(string linkName)
        {
            Details.LinkName = linkName;
            return this;
        }

        public FluentCommand Link(ODataExpression expression)
        {
            Details.LinkExpression = expression;
            return this;
        }

        public FluentCommand Key(params object[] key)
        {
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
            Details.KeyValues = key.ToList();
            Details.NamedKeyValues = null;
            Details.IsAlternateKey = false;
            return this;
        }

        public FluentCommand Key(IDictionary<string, object> key)
        {
            Details.KeyValues = null;
            Details.NamedKeyValues = key;
            Details.IsAlternateKey = false;
            return this;
        }

        public FluentCommand Filter(string filter)
        {
            if (string.IsNullOrEmpty(Details.Filter))
                Details.Filter = filter;
            else
                Details.Filter = $"({Details.Filter}) and ({filter})";
            return this;
        }

        public FluentCommand Filter(ODataExpression expression)
        {
            if (ReferenceEquals(Details.FilterExpression, null))
                Details.FilterExpression = expression;
            else
                Details.FilterExpression = Details.FilterExpression && expression;
            return this;
        }

        public FluentCommand Search(string search)
        {
            Details.Search = search;
            return this;
        }

        public FluentCommand Skip(long count)
        {
            Details.SkipCount = count;
            return this;
        }

        public FluentCommand Top(long count)
        {
            if (!Details.HasKey || Details.HasFunction)
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
            Details.ExpandAssociations.AddRange(new[] { new KeyValuePair<string, ODataExpandOptions>("*", ODataExpandOptions.ByValue()) });
            return this;
        }

        public FluentCommand Expand(IEnumerable<string> associations)
        {
            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, ODataExpandOptions.ByValue())));
            return this;
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions, IEnumerable<string> associations)
        {
            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, expandOptions)));
            return this;
        }

        public FluentCommand Expand(params string[] associations)
        {
            Details.ExpandAssociations.AddRange(SplitItems(associations).Select(x => new KeyValuePair<string, ODataExpandOptions>(x, ODataExpandOptions.ByValue())));
            return this;
        }

        public FluentCommand Expand(ODataExpandOptions expandOptions, params string[] associations)
        {
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
            Details.SelectColumns.AddRange(SplitItems(columns).ToList());
            return this;
        }

        public FluentCommand Select(params string[] columns)
        {
            Details.SelectColumns.AddRange(SplitItems(columns).ToList());
            return this;
        }

        public FluentCommand Select(params ODataExpression[] columns)
        {
            return Select(columns.Select(x => x.Reference));
        }

        public FluentCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
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
            Details.OrderbyColumns.AddRange(SplitItems(columns).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public FluentCommand ThenByDescending(params ODataExpression[] columns)
        {
            return ThenByDescending(columns.Select(x => x.Reference).ToArray());
        }

        public FluentCommand QueryOptions(string queryOptions)
        {
            if (Details.QueryOptions == null)
                Details.QueryOptions = queryOptions;
            else
                Details.QueryOptions = $"{Details.QueryOptions}&{queryOptions}";
            return this;
        }

        public FluentCommand QueryOptions(IDictionary<string, object> queryOptions)
        {
            Details.QueryOptionsKeyValues = queryOptions;
            return this;
        }

        public FluentCommand QueryOptions(ODataExpression expression)
        {
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
            Details.MediaName = streamName;
            return this;
        }

        public FluentCommand Media(ODataExpression expression)
        {
            return Media(expression.Reference);
        }

        public FluentCommand Count()
        {
            Details.ComputeCount = true;
            return this;
        }

        public FluentCommand Set(object value)
        {
            Details.EntryData = TypeCache.ToDictionary(value);
            Details.BatchEntries?.GetOrAdd(value, Details.EntryData);
            return this;
        }

        public FluentCommand Set(IDictionary<string, object> value)
        {
            Details.EntryData = value;
            return this;
        }

        public FluentCommand Set(params ODataExpression[] value)
        {
            Details.EntryData = value.Select(x => new KeyValuePair<string, object>(x.Reference, x.Value)).ToIDictionary();
            Details.BatchEntries?.GetOrAdd(value, Details.EntryData);
            return this;
        }

        public FluentCommand Function(string functionName)
        {
            Details.FunctionName = functionName;
            return this;
        }

        public FluentCommand Action(string actionName)
        {
            Details.ActionName = actionName;
            return this;
        }

        public FluentCommand WithCount()
        {
            Details.IncludeCount = true;
            return this;
        }

        internal IList<string> SelectedColumns => Details.SelectColumns;

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
