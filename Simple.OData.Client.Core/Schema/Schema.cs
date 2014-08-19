using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class Schema
    {
        private static readonly SimpleDictionary<string, Schema> Instances = new SimpleDictionary<string, Schema>();

        private readonly SchemaProvider _schemaProvider;
        private readonly Func<Task<string>> _resolveMetadataAsync;
        private Func<ProviderMetadata> _createProviderMetadata;
        private string _metadataString;

        private Lazy<ProviderMetadata> _lazyProviderMetadata;
        private Lazy<Collection<EntitySet>> _lazyEntitySets;

        private Schema(string metadataString, Func<Task<string>> resolveMedatataAsync)
        {
            ResetCache();
            _schemaProvider = new SchemaProvider();

            _metadataString = metadataString;
            _resolveMetadataAsync = resolveMedatataAsync;

            if (_resolveMetadataAsync == null)
            {
                _createProviderMetadata = () => _schemaProvider.ParseMetadata(_metadataString);
            }
        }

        private Schema(SchemaProvider schemaProvider)
        {
            ResetCache();

            _schemaProvider = schemaProvider;
        }

        internal void ResetCache()
        {
            _metadataString = null;

            _lazyProviderMetadata = new Lazy<ProviderMetadata>(CreateProviderMetadata);
            _lazyEntitySets = new Lazy<Collection<EntitySet>>(CreateEntitySetCollection);
        }

        public async Task<Schema> ResolveAsync(CancellationToken cancellationToken)
        {
            if (_metadataString == null)
            {
                if (_schemaProvider != null)
                {
                    var response = await _schemaProvider.SendSchemaRequestAsync(cancellationToken);
                    _metadataString = await _schemaProvider.GetSchemaAsStringAsync(response);
                    var providerMetadata = await _schemaProvider.GetMetadataAsync(response);
                    _createProviderMetadata = () => providerMetadata;
                    var metadata = await _schemaProvider.GetSchemaAsync(providerMetadata);
                }
                else
                {
                    _metadataString = await _resolveMetadataAsync();
                }
            }

            _lazyProviderMetadata = new Lazy<ProviderMetadata>(CreateProviderMetadata);
            return this;
        }

        public ProviderMetadata ProviderMetadata
        {
            get { return _lazyProviderMetadata.Value; }
        }

        public string MetadataAsString
        {
            get { return _metadataString; }
        }

        public IEnumerable<EntitySet> EntitySets
        {
            get { return _lazyEntitySets.Value.AsEnumerable(); }
        }

        public bool HasEntitySet(string entitySetName)
        {
            return _lazyEntitySets.Value.Any(x => ProviderMetadata.NamesAreEqual(x.ActualName, entitySetName));
        }

        public EntitySet FindEntitySet(string entitySetName)
        {
            var actualName = ProviderMetadata.GetEntitySetExactName(entitySetName);
            return _lazyEntitySets.Value.Single(x => x.ActualName == actualName);
        }

        public EntitySet FindBaseEntitySet(string entitySetPath)
        {
            return this.FindEntitySet(entitySetPath.Split('/').First());
        }

        public EntitySet FindConcreteEntitySet(string entitySetPath)
        {
            var items = entitySetPath.Split('/');
            if (items.Count() > 1)
            {
                var baseEntitySet = this.FindEntitySet(items[0]);
                var entitySet = string.IsNullOrEmpty(items[1])
                    ? baseEntitySet
                    : baseEntitySet.FindDerivedEntitySet(items[1]);
                return entitySet;
            }
            else
            {
                return this.FindEntitySet(entitySetPath);
            }
        }

        private ProviderMetadata CreateProviderMetadata()
        {
            return _createProviderMetadata();
        }

        private Collection<EntitySet> CreateEntitySetCollection()
        {
            return new Collection<EntitySet>(ProviderMetadata.GetEntitySetNames().Select(x => new EntitySet(x, null, this)).ToList());
        }

        internal static Schema FromUrl(string urlBase, ICredentials credentials = null)
        {
            return Instances.GetOrAdd(urlBase, new Schema(new SchemaProvider(urlBase, credentials)));
        }

        internal static Schema FromMetadata(string metadataString)
        {
            return new Schema(metadataString, null);
        }

        internal static void Add(string urlBase, Schema schema)
        {
            Instances.GetOrAdd(urlBase, sp => schema);
        }

        internal static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
