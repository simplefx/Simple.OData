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

        private readonly ProviderFactory _providerFactory;
        private readonly Func<Task<string>> _resolveMetadataAsync;
        private Func<ODataProvider> _createProvider;
        private string _metadataString;

        private Lazy<ODataProvider> _lazyProvider;
        private Lazy<Collection<EntitySet>> _lazyEntitySets;

        private Schema(string urlBase, string metadataString, Func<Task<string>> resolveMedatataAsync)
        {
            ResetCache();
            _providerFactory = new ProviderFactory(urlBase, null);

            _metadataString = metadataString;
            _resolveMetadataAsync = resolveMedatataAsync;

            if (_resolveMetadataAsync == null)
            {
                _createProvider = () => _providerFactory.ParseMetadata(_metadataString);
            }
        }

        private Schema(ProviderFactory providerFactory)
        {
            ResetCache();

            _providerFactory = providerFactory;
        }

        internal void ResetCache()
        {
            _metadataString = null;

            _lazyProvider = new Lazy<ODataProvider>(CreateProvider);
            _lazyEntitySets = new Lazy<Collection<EntitySet>>(CreateEntitySetCollection);
        }

        public async Task<Schema> ResolveAsync(CancellationToken cancellationToken)
        {
            if (_metadataString == null)
            {
                if (_providerFactory != null)
                {
                    var response = await _providerFactory.SendSchemaRequestAsync(cancellationToken);
                    _metadataString = await _providerFactory.GetSchemaAsStringAsync(response);
                    var provider = await _providerFactory.GetMetadataAsync(response);
                    _createProvider = () => provider;
                    var metadata = await _providerFactory.GetSchemaAsync(provider);
                }
                else
                {
                    _metadataString = await _resolveMetadataAsync();
                }
            }

            _lazyProvider = new Lazy<ODataProvider>(CreateProvider);
            return this;
        }

        public ODataProvider Provider
        {
            get { return _lazyProvider.Value; }
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
            return _lazyEntitySets.Value.Any(x => Utils.NamesAreEqual(x.ActualName, entitySetName));
        }

        public EntitySet FindEntitySet(string entitySetName)
        {
            var actualName = Provider.GetMetadata().GetEntitySetExactName(entitySetName);
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

        private ODataProvider CreateProvider()
        {
            return _createProvider();
        }

        private Collection<EntitySet> CreateEntitySetCollection()
        {
            return new Collection<EntitySet>(Provider.GetMetadata().GetEntitySetNames().Select(x => new EntitySet(x, null, this)).ToList());
        }

        internal static Schema FromUrl(string urlBase, ICredentials credentials = null)
        {
            return Instances.GetOrAdd(urlBase, new Schema(new ProviderFactory(urlBase, credentials)));
        }

        internal static Schema FromMetadata(string urlBase, string metadataString)
        {
            return new Schema(urlBase, metadataString, null);
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
