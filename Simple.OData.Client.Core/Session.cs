using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class Session : ISession
    {
        private readonly AdapterFactory _adapterFactory;
        private Func<IODataAdapter> _createAdapter;
        private IODataAdapter _adapter;

        public string UrlBase { get; private set; }
        public ICredentials Credentials { get; private set; }
        public ODataPayloadFormat PayloadFormat { get; private set; }
        public MetadataCache MetadataCache { get; private set; }
        public IPluralizer Pluralizer { get; internal set; }

        private Session(string urlBase, string metadataString)
        {
            _adapterFactory = new AdapterFactory(this);
            _createAdapter = () => _adapterFactory.ParseMetadata(metadataString);

            this.UrlBase = urlBase;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(urlBase, new MetadataCache());
            this.MetadataCache.SetMetadataString(metadataString);
            this.Pluralizer = new SimplePluralizer();
        }

        private Session(string urlBase, ICredentials credentials, ODataPayloadFormat payloadFormat)
        {
            _adapterFactory = new AdapterFactory(this);
            _createAdapter = () => _adapterFactory.ParseMetadata(this.MetadataCache.MetadataAsString);

            this.UrlBase = urlBase;
            this.Credentials = credentials;
            this.PayloadFormat = payloadFormat;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(urlBase, new MetadataCache());
            this.Pluralizer = new SimplePluralizer();
        }

        public void ResetMetadataCache()
        {
            MetadataCache.Instances.Remove(MetadataCache.Instances.Single(x => x.Value == this.MetadataCache).Key);
        }

        public async Task<IODataAdapter> ResolveAdapterAsync(CancellationToken cancellationToken)
        {
            if (!this.MetadataCache.IsResolved())
            {
                var response = await _adapterFactory.SendMetadataRequestAsync(cancellationToken);
                this.MetadataCache.SetMetadataString(await _adapterFactory.GetMetadataAsStringAsync(response));

                var adapter = await _adapterFactory.CreateAdapterAsync(response);
                _createAdapter = () => adapter;
            }

            if (this.PayloadFormat == ODataPayloadFormat.Unspecified)
                this.PayloadFormat = this.Adapter.DefaultPayloadFormat;

            return this.Adapter;
        }

        public IODataAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                    _adapter = _createAdapter();
                return _adapter;
            }
        }

        public IMetadata Metadata
        {
            get { return this.Adapter.GetMetadata(); }
        }

        public static Session FromSettings(ODataClientSettings settings)
        {
            return new Session(settings.UrlBase, settings.Credentials, settings.PayloadFormat);
        }

        public static Session FromMetadata(string urlBase, string metadataString)
        {
            return new Session(urlBase, metadataString);
        }
    }
}
