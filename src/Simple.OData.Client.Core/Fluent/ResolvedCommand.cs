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
    public partial class ResolvedCommand
    {
        private readonly FluentCommand _command;
        private readonly Session _session;
        private readonly CommandDetails _details;

        internal ResolvedCommand(FluentCommand command, Session session)
        {
            _command = command;
            _session = session;
            _details = new CommandDetails(_command.Details);

            EvaluateCollectionName();
            EvaluateDerivedCollectionName();
            EvaluateNamedKeyValues();
            EvaluateFilter();
            EvaluateLinkName();
        }

        public FluentCommand Source => _command; 

        private bool IsBatchResponse => _session == null;

        internal ITypeCache TypeCache => _session.TypeCache;

        internal CommandDetails Details => _details;

        internal EntityCollection EntityCollection
        {
            get
            {
                if (string.IsNullOrEmpty(_details.CollectionName) && string.IsNullOrEmpty(_details.LinkName))
                    return null;

                EntityCollection entityCollection;
                if (!string.IsNullOrEmpty(_details.LinkName))
                {
                    var parent = new FluentCommand(_details.Parent).Resolve();
                    var collectionName = _session.Metadata.GetNavigationPropertyPartnerTypeName(
                        parent.EntityCollection.Name, _details.LinkName);
                    entityCollection = _session.Metadata.GetEntityCollection(collectionName);
                }
                else
                {
                    entityCollection = _session.Metadata.GetEntityCollection(_details.CollectionName);
                }

                return string.IsNullOrEmpty(_details.DerivedCollectionName)
                    ? entityCollection
                    : _session.Metadata.GetDerivedEntityCollection(entityCollection, _details.DerivedCollectionName);
            }
        }

        public string QualifiedEntityCollectionName
        {
            get
            {
                var entityCollection = this.EntityCollection;
                return entityCollection.BaseEntityCollection == null
                    ? entityCollection.Name
                    : $"{entityCollection.BaseEntityCollection.Name}/{_session.Metadata.GetQualifiedTypeName(entityCollection.Name)}";
            }
        }

        public string DynamicPropertiesContainerName => _details.DynamicPropertiesContainerName;

        private void EvaluateCollectionName()
        {
            if (IsBatchResponse) return;

            if (_details.CollectionName == null && !ReferenceEquals(_command.Details.CollectionExpression, null))
            {
                var collectionName = _command.Details.CollectionExpression.AsString(_session);
                var items = collectionName.Split('/');
                if (items.Count() > 1)
                {
                    _details.CollectionName = items[0];
                    _details.DerivedCollectionName = items[1];
                }
                else
                {
                    _details.CollectionName = collectionName;
                }
            }
        }

        private void EvaluateDerivedCollectionName()
        {
            if (IsBatchResponse) return;

            if (_details.DerivedCollectionName == null && !ReferenceEquals(_command.Details.DerivedCollectionExpression, null))
            {
                var derivedCollectionName = _command.Details.DerivedCollectionExpression.AsString(_session);
                _details.DerivedCollectionName = derivedCollectionName;
            }
        }

        public void EvaluateNamedKeyValues()
        {
            if (_details.NamedKeyValues != null)
            {
                if (NamedKeyValuesMatchAnyKey(_details.NamedKeyValues, out var matchingKey, out bool isAlternateKey))
                {
                    _details.NamedKeyValues = matchingKey.ToDictionary();
                    _details.IsAlternateKey = isAlternateKey;
                }
                else if (TryExtractKeyFromNamedValues(_details.NamedKeyValues, out var containedKey))
                {
                    _details.NamedKeyValues = containedKey.ToDictionary();
                }
                else
                {
                    _details.NamedKeyValues = null;
                }
            }
        }

        public void EvaluateFilter()
        {
            if (_details.Filter == null && !ReferenceEquals(_command.Details.FilterExpression, null))
            {
                _details.NamedKeyValues = TryInterpretFilterExpressionAsKey(_command.Details.FilterExpression, out var isAlternateKey);
                _details.IsAlternateKey = isAlternateKey;

                if (_details.NamedKeyValues == null)
                {
                    var entityCollection = this.EntityCollection;
                    if (_command.HasFunction)
                    {
                        var collection = _session.Metadata.GetFunctionReturnCollection(_command.FunctionName);
                        if (collection != null)
                        {
                            entityCollection = collection;
                        }
                    }
                    _details.Filter = _command.Details.FilterExpression.Format(
                        new ExpressionContext(_session, entityCollection, null, this.DynamicPropertiesContainerName));
                }
                else
                {
                    _details.KeyValues = null;
                    _details.TopCount = -1;
                }
                if (_command.Details.FilterExpression.HasTypeConstraint(_command.Details.DerivedCollectionName))
                {
                    _details.DerivedCollectionName = null;
                }
            }
        }

        public void EvaluateLinkName()
        {
            if (IsBatchResponse) return;

            if (_details.LinkName == null && !ReferenceEquals(_command.Details.LinkExpression, null))
            {
                _details.LinkName = _command.Details.LinkExpression.AsString(_session);
            }
        }

        public ResolvedCommand Key(IDictionary<string, object> key)
        {
            if (IsBatchResponse) return this;

            _details.KeyValues = null;
            _details.NamedKeyValues = null;
            _details.IsAlternateKey = false;

            if (NamedKeyValuesMatchAnyKey(key, out var matchingKey, out bool isAlternateKey))
            {
                _details.NamedKeyValues = matchingKey.ToDictionary();
                _details.IsAlternateKey = isAlternateKey;
            }
            else if (TryExtractKeyFromNamedValues(key, out var containedKey))
            {
                _details.NamedKeyValues = containedKey.ToDictionary();
            }

            return this;
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            return Format();
        }

        public bool FilterIsKey => _details.NamedKeyValues != null;

        public IDictionary<string, object> FilterAsKey => _details.NamedKeyValues;

        public ResolvedCommand WithCount()
        {
            if (IsBatchResponse) return this;

            _details.IncludeCount = true;
            return this;
        }

        public override string ToString()
        {
            return Format();
        }

        private string Format()
        {
            return _session.Adapter.GetCommandFormatter().FormatCommand(this);
        }

        internal bool HasKey => _details.KeyValues != null && _details.KeyValues.Count > 0 || _details.NamedKeyValues != null && _details.NamedKeyValues.Count > 0;

        internal bool HasFilter => !string.IsNullOrEmpty(_details.Filter) || !ReferenceEquals(_details.FilterExpression, null);

        internal bool HasSearch => !string.IsNullOrEmpty(_details.Search);

        public bool HasFunction => !string.IsNullOrEmpty(_details.FunctionName);

        public bool HasAction => !string.IsNullOrEmpty(_details.ActionName);

        internal IDictionary<string, object> KeyValues
        {
            get
            {
                if (!HasKey)
                    return null;

                var keyNames = _session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();
                var namedKeyValues = _details.KeyValues?.Zip(
                     keyNames, 
                     (keyValue, keyName) => new KeyValuePair<string, object>(keyName, keyValue))
                    ??
                    _details.NamedKeyValues;
                return namedKeyValues?.ToDictionary(
                    x => x.Key,
                    x => x.Value is ODataExpression oDataExpression ? oDataExpression.Value : x.Value);
            }
        }

        internal IDictionary<string, object> CommandData
        {
            get
            {
                if (_details.EntryData == null)
                    return new Dictionary<string, object>();
                if (string.IsNullOrEmpty(_details.DynamicPropertiesContainerName))
                    return _details.EntryData;

                var entryData = new Dictionary<string, object>();
                foreach (var key in _details.EntryData.Keys.Where(x =>
                    !string.Equals(x, _details.DynamicPropertiesContainerName, StringComparison.OrdinalIgnoreCase)))
                {
                    entryData.Add(key, _details.EntryData[key]);
                }

                if (_details.EntryData.TryGetValue(_details.DynamicPropertiesContainerName, out var dynamicProperties) && dynamicProperties != null)
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
                        throw new InvalidOperationException($"Property {_details.DynamicPropertiesContainerName} must implement IDictionary<string,object> interface");
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
            var keyNames = _session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name).ToList();

            return Utils.NamedKeyValuesMatchKeyNames(namedKeyValues, _session.Settings.NameMatchResolver, keyNames, out matchingNamedKeyValues);
        }

        private bool NamedKeyValuesMatchAlternateKey(IDictionary<string, object> namedKeyValues, out IEnumerable<KeyValuePair<string, object>> alternateKeyNamedValues)
        {
            alternateKeyNamedValues = null;

            var alternateKeys = _session.Metadata.GetAlternateKeyPropertyNames(this.EntityCollection.Name).ToList();

            foreach (var alternateKey in alternateKeys)
            {
                if (Utils.NamedKeyValuesMatchKeyNames(namedKeyValues, _session.Settings.NameMatchResolver, alternateKey, out alternateKeyNamedValues))
                    return true;
            }

            return false;
        }

        private bool TryExtractKeyFromNamedValues(IDictionary<string, object> namedValues, out IEnumerable<KeyValuePair<string, object>> matchingNamedKeyValues)
        {
            return Utils.NamedKeyValuesContainKeyNames(namedValues,
                _session.Settings.NameMatchResolver,
                _session.Metadata.GetDeclaredKeyPropertyNames(this.EntityCollection.Name),
                out matchingNamedKeyValues);
        }
    }
}
