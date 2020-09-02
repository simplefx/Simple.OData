using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class ResolvedCommand
    {
        internal ResolvedCommand(FluentCommand command, Session session)
        {
            Session = session;
            Details = new FluentCommandDetails(command.Details);

            ResolveCollectionName(command.Details);
            ResolveDerivedCollectionName(command.Details);
            ResolveLinkName(command.Details);
            ResolveNamedKeyValues(command.Details);
            ResolveFilter(command.Details);
        }

        internal Session Session { get; private set; }

        internal FluentCommandDetails Details { get; private set; }

        internal ITypeCache TypeCache => this.Session.TypeCache;

        internal EntityCollection EntityCollection
        {
            get
            {
                if (string.IsNullOrEmpty(Details.CollectionName) && string.IsNullOrEmpty(Details.LinkName))
                    return null;

                EntityCollection entityCollection;
                if (!string.IsNullOrEmpty(Details.LinkName))
                {
                    var parent = new FluentCommand(this.Session, Details.Parent).Resolve(this.Session);
                    var collectionName = this.Session.Metadata.GetNavigationPropertyPartnerTypeName(
                        parent.EntityCollection.Name, Details.LinkName);
                    entityCollection = this.Session.Metadata.GetEntityCollection(collectionName);
                }
                else
                {
                    entityCollection = this.Session.Metadata.GetEntityCollection(Details.CollectionName);
                }

                return string.IsNullOrEmpty(Details.DerivedCollectionName)
                    ? entityCollection
                    : this.Session.Metadata.GetDerivedEntityCollection(entityCollection, Details.DerivedCollectionName);
            }
        }

        public string QualifiedEntityCollectionName
        {
            get
            {
                var entityCollection = this.EntityCollection;
                return entityCollection.BaseEntityCollection == null
                    ? entityCollection.Name
                    : $"{entityCollection.BaseEntityCollection.Name}/{this.Session.Metadata.GetQualifiedTypeName(entityCollection.Name)}";
            }
        }

        public string DynamicPropertiesContainerName => Details.DynamicPropertiesContainerName;

        private void ResolveCollectionName(FluentCommandDetails details)
        {
            if (Details.CollectionName == null && !ReferenceEquals(details.CollectionExpression, null))
            {
                var collectionName = details.CollectionExpression.AsString(this.Session);
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
            }
        }

        private void ResolveDerivedCollectionName(FluentCommandDetails details)
        {
            if (Details.DerivedCollectionName == null && !ReferenceEquals(details.DerivedCollectionExpression, null))
            {
                var derivedCollectionName = details.DerivedCollectionExpression.AsString(this.Session);
                Details.DerivedCollectionName = derivedCollectionName;
            }
        }

        private void ResolveLinkName(FluentCommandDetails details)
        {
            if (Details.LinkName == null && !ReferenceEquals(details.LinkExpression, null))
            {
                Details.LinkName = details.LinkExpression.AsString(this.Session);
            }
        }

        private void ResolveNamedKeyValues(FluentCommandDetails details)
        {
            if (Details.NamedKeyValues != null)
            {
                if (NamedKeyValuesMatchAnyKey(Details.NamedKeyValues, out var matchingKey, out bool isAlternateKey))
                {
                    Details.NamedKeyValues = matchingKey.ToDictionary();
                    Details.IsAlternateKey = isAlternateKey;
                }
                else if (TryExtractKeyFromNamedValues(Details.NamedKeyValues, out var containedKey))
                {
                    Details.NamedKeyValues = containedKey.ToDictionary();
                }
                else
                {
                    Details.NamedKeyValues = null;
                }
            }
        }

        private void ResolveFilter(FluentCommandDetails details)
        {
            if (Details.Filter == null && !ReferenceEquals(details.FilterExpression, null))
            {
                Details.NamedKeyValues = TryInterpretFilterExpressionAsKey(details.FilterExpression, out var isAlternateKey);
                Details.IsAlternateKey = isAlternateKey;

                if (Details.NamedKeyValues == null)
                {
                    var entityCollection = this.EntityCollection;
                    if (details.HasFunction)
                    {
                        var collection = this.Session.Metadata.GetFunctionReturnCollection(details.FunctionName);
                        if (collection != null)
                        {
                            entityCollection = collection;
                        }
                    }
                    Details.Filter = details.FilterExpression.Format(
                        new ExpressionContext(this.Session, entityCollection, null, this.DynamicPropertiesContainerName));
                }
                else
                {
                    Details.KeyValues = null;
                    Details.TopCount = -1;
                }
                if (details.FilterExpression.HasTypeConstraint(details.DerivedCollectionName))
                {
                    Details.DerivedCollectionName = null;
                }
            }
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            await this.Session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            return Format();
        }

        public IDictionary<string, object> FilterAsKey => Details.NamedKeyValues;

        public ResolvedCommand WithCount()
        {
            Details.IncludeCount = true;
            return this;
        }

        public override string ToString()
        {
            return Format();
        }

        private string Format()
        {
            return this.Session.Adapter.GetCommandFormatter().FormatCommand(this);
        }

        internal IDictionary<string, object> KeyValues
        {
            get
            {
                if (!Details.HasKey)
                    return null;

                var keyNames = this.Session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();
                var namedKeyValues = Details.KeyValues?.Zip(
                     keyNames, 
                     (keyValue, keyName) => new KeyValuePair<string, object>(keyName, keyValue))
                    ??
                    Details.NamedKeyValues;
                return namedKeyValues?.ToDictionary(
                    x => x.Key,
                    x => x.Value is ODataExpression oDataExpression ? oDataExpression.Value : x.Value);
            }
        }

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

        private IDictionary<string, object> TryInterpretFilterExpressionAsKey(ODataExpression expression, out bool isAlternateKey)
        {
            isAlternateKey = false;
            var ok = false;

            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (!ReferenceEquals(expression, null))
            {
                ok = expression.ExtractLookupColumns(namedKeyValues);
            }
            if (!ok)
                return null;

            if (NamedKeyValuesMatchAnyKey(namedKeyValues, out var matchingNamedKeyValues, out isAlternateKey))
                return matchingNamedKeyValues.ToIDictionary();

            return null;
        }

        private bool NamedKeyValuesMatchAnyKey(IDictionary<string, object> namedKeyValues, out IEnumerable<KeyValuePair<string, object>> matchingNamedKeyValues, out bool isAlternateKey)
        {
            isAlternateKey = false;

            if (NamedKeyValuesMatchPrimaryKey(namedKeyValues, out matchingNamedKeyValues))
                return true;

            if (NamedKeyValuesMatchAlternateKey(namedKeyValues, out matchingNamedKeyValues))
            {
                isAlternateKey = true;
                return true;
            }

            return false;
        }

        private bool NamedKeyValuesMatchPrimaryKey(IDictionary<string, object> namedKeyValues, out IEnumerable<KeyValuePair<string, object>> matchingNamedKeyValues)
        {
            var keyNames = this.Session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();

            return Utils.NamedKeyValuesMatchKeyNames(namedKeyValues, this.Session.Settings.NameMatchResolver, keyNames, out matchingNamedKeyValues);
        }

        private bool NamedKeyValuesMatchAlternateKey(IDictionary<string, object> namedKeyValues, out IEnumerable<KeyValuePair<string, object>> alternateKeyNamedValues)
        {
            alternateKeyNamedValues = null;

            var alternateKeys = this.Session.Metadata.GetAlternateKeyPropertyNames(this.EntityCollection.Name).ToList();

            foreach (var alternateKey in alternateKeys)
            {
                if (Utils.NamedKeyValuesMatchKeyNames(namedKeyValues, this.Session.Settings.NameMatchResolver, alternateKey, out alternateKeyNamedValues))
                    return true;
            }

            return false;
        }

        private bool TryExtractKeyFromNamedValues(IDictionary<string, object> namedValues, out IEnumerable<KeyValuePair<string, object>> matchingNamedKeyValues)
        {
            return Utils.NamedKeyValuesContainKeyNames(namedValues,
                this.Session.Settings.NameMatchResolver,
                this.Session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name),
                out matchingNamedKeyValues);
        }
    }
}
