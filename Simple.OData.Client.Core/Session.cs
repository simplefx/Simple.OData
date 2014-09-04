using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class Session
    {
        private static readonly SimpleDictionary<string, Session> Instances = new SimpleDictionary<string, Session>();

        private readonly ProviderFactory _providerFactory;
        private readonly Func<Task<string>> _resolveMetadataAsync;
        private Func<ODataProvider> _createProvider;
        private string _metadataString;

        private Lazy<ODataProvider> _lazyProvider;
        private Lazy<Collection<EntitySet>> _lazyEntitySets;

        private Session(string urlBase, string metadataString, Func<Task<string>> resolveMedatataAsync)
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

        private Session(ProviderFactory providerFactory)
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

        public async Task<Session> ResolveAsync(CancellationToken cancellationToken)
        {
            if (_metadataString == null)
            {
                if (_providerFactory != null)
                {
                    var response = await _providerFactory.SendSchemaRequestAsync(cancellationToken);
                    _metadataString = await _providerFactory.GetMetadataAsStringAsync(response);
                    var provider = await _providerFactory.GetMetadataAsync(response);
                    _createProvider = () => provider;
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

        internal static Session FromUrl(string urlBase, ICredentials credentials = null)
        {
            return Instances.GetOrAdd(urlBase, new Session(new ProviderFactory(urlBase, credentials)));
        }

        internal static Session FromMetadata(string urlBase, string metadataString)
        {
            return new Session(urlBase, metadataString, null);
        }

        internal static void Add(string urlBase, Session session)
        {
            Instances.GetOrAdd(urlBase, sp => session);
        }

        internal static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
