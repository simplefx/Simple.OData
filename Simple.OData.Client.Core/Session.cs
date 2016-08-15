using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class Session : ISession
    {
        private readonly AdapterFactory _adapterFactory;
        private Func<IODataAdapter> _createAdapter;
        private IODataAdapter _adapter;
        private HttpConnection _httpConnection;

        public ODataClientSettings Settings { get; private set; }
        public MetadataCache MetadataCache { get; private set; }
        public IPluralizer Pluralizer { get; internal set; }

        private Session(Uri baseUri, string metadataString)
        {
            _adapterFactory = new AdapterFactory(this);
            _createAdapter = () => _adapterFactory.ParseMetadata(metadataString);

            this.Settings = new ODataClientSettings();
            this.Settings.BaseUri = baseUri;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(baseUri.AbsoluteUri, new MetadataCache());
            this.MetadataCache.SetMetadataDocument(metadataString);
            this.Pluralizer = new SimplePluralizer();
        }

        private Session(ODataClientSettings settings)
        {
            if (settings.BaseUri == null || string.IsNullOrEmpty(settings.BaseUri.AbsoluteUri))
                throw new InvalidOperationException("Unable to create client session with no URI specified");

            _adapterFactory = new AdapterFactory(this);
            _createAdapter = () => _adapterFactory.ParseMetadata(this.MetadataCache.MetadataDocument);

            this.Settings = settings;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(this.Settings.BaseUri.AbsoluteUri, new MetadataCache());
            this.Pluralizer = new SimplePluralizer();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_httpConnection != null)
                {
                    _httpConnection.Dispose();
                    _httpConnection = null;
                }
            }
        }

        public void Trace(string message, params object[] messageParams)
        {
            if (this.Settings.OnTrace != null)
            {
                this.Settings.OnTrace(message, messageParams);
            }
        }

        public void ClearMetadataCache()
        {
            MetadataCache.Instances.Remove(MetadataCache.Instances.Single(x => x.Value == this.MetadataCache).Key);
        }

        public async Task<IODataAdapter> ResolveAdapterAsync(CancellationToken cancellationToken)
        {
            if (!this.MetadataCache.IsResolved())
            {
                IODataAdapter adapter;
                if (string.IsNullOrEmpty(this.Settings.MetadataDocument))
                {
                    var response = await SendMetadataRequestAsync(cancellationToken).ConfigureAwait(false);
                    this.MetadataCache.SetMetadataDocument(await _adapterFactory.GetMetadataDocumentAsync(response).ConfigureAwait(false));
                    adapter = await _adapterFactory.CreateAdapterAsync(response).ConfigureAwait(false);
                }
                else
                {
                    this.MetadataCache.SetMetadataDocument(this.Settings.MetadataDocument);
                    adapter = _adapterFactory.CreateAdapter(this.Settings.MetadataDocument);
                }
                _createAdapter = () => adapter;
            }

            if (this.Settings.PayloadFormat == ODataPayloadFormat.Unspecified)
                this.Settings.PayloadFormat = this.Adapter.DefaultPayloadFormat;

            return this.Adapter;
        }

        public IODataAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    lock (this)
                    {
                        if (_adapter == null)
                            _adapter = _createAdapter();
                    }
                }
                return _adapter;
            }
        }

        public IMetadata Metadata
        {
            get { return this.Adapter.GetMetadata(); }
        }

        public HttpConnection GetHttpConnection()
        {
            if (_httpConnection == null)
            {
                lock (this)
                {
                    if (_httpConnection == null)
                        _httpConnection = new HttpConnection(this.Settings);
                }
            }

            return _httpConnection;
        }

        internal static Session FromSettings(ODataClientSettings settings)
        {
            return new Session(settings);
        }

        internal static Session FromMetadata(Uri baseUri, string metadataString)
        {
            return new Session(baseUri, metadataString);
        }

        private async Task<HttpResponseMessage> SendMetadataRequestAsync(CancellationToken cancellationToken)
        {
            var request = new ODataRequest(RestVerbs.Get, this, ODataLiteral.Metadata);
            return await new RequestRunner(this).ExecuteRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
