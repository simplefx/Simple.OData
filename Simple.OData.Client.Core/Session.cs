using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class Session : ISession
    {
        private readonly ProviderFactory _providerFactory;
        private Func<ODataProvider> _createProvider;
        private ODataProvider _provider;

        public string UrlBase { get; private set; }
        public ICredentials Credentials { get; private set; }
        public MetadataCache MetadataCache { get; private set; }
        public IPluralizer Pluralizer { get; internal set; }

        private Session(string urlBase, string metadataString)
        {
            _providerFactory = new ProviderFactory(this);
            _createProvider = () => _providerFactory.ParseMetadata(metadataString);

            this.UrlBase = urlBase;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(urlBase, new MetadataCache(_createProvider));
            this.MetadataCache.SetMetadataString(metadataString);
            this.Pluralizer = new SimplePluralizer();
        }

        private Session(string urlBase, ICredentials credentials)
        {
            _providerFactory = new ProviderFactory(this);
            _createProvider = () => _providerFactory.ParseMetadata(this.MetadataCache.MetadataAsString);

            this.UrlBase = urlBase;
            this.Credentials = credentials;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(urlBase, new MetadataCache(_createProvider));
            this.Pluralizer = new SimplePluralizer();
        }

        public void ResetMetadataCache()
        {
            MetadataCache.Instances.Remove(MetadataCache.Instances.Single(x => x.Value == this.MetadataCache).Key);
        }

        public async Task<ODataProvider> ResolveProviderAsync(CancellationToken cancellationToken)
        {
            if (!this.MetadataCache.IsResolved())
            {
                var response = await _providerFactory.SendMetadataRequestAsync(cancellationToken);
                this.MetadataCache.SetMetadataString(await _providerFactory.GetMetadataAsStringAsync(response));

                var provider = await _providerFactory.CreateProviderAsync(response);
                _createProvider = () => provider;
            }

            return this.Provider;
        }

        public ODataProvider Provider
        {
            get
            {
                if (_provider == null)
                    _provider = _createProvider();
                return _provider;
            }
        }

        public static Session FromUrl(string urlBase, ICredentials credentials = null)
        {
            return new Session(urlBase, credentials);
        }

        public static Session FromMetadata(string urlBase, string metadataString)
        {
            return new Session(urlBase, metadataString);
        }
    }
}
