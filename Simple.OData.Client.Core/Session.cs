using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private HttpMessageHandler _messageHandler;
        private HttpClient _httpClient;

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

        private Session(Uri baseUri, ICredentials credentials, ODataPayloadFormat payloadFormat)
        {
            _adapterFactory = new AdapterFactory(this);
            _createAdapter = () => _adapterFactory.ParseMetadata(this.MetadataCache.MetadataDocument);

            this.Settings = new ODataClientSettings();
            this.Settings.BaseUri = baseUri;
            this.Settings.Credentials = credentials;
            this.Settings.PayloadFormat = payloadFormat;
            this.MetadataCache = MetadataCache.Instances.GetOrAdd(baseUri.AbsoluteUri, new MetadataCache());
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
            DisposeHttpClient();
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
                    var response = await _adapterFactory.SendMetadataRequestAsync(cancellationToken);
                    this.MetadataCache.SetMetadataDocument(await _adapterFactory.GetMetadataDocumentAsync(response));
                    adapter = await _adapterFactory.CreateAdapterAsync(response);
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

        public HttpClient GetHttpClient()
        {
            if (_httpClient != null && this.Settings.HttpClientLifetime == HttpClientLifetime.PerCall)
            {
                DisposeHttpClient();
            }

            if (_httpClient == null)
            {
                CreateHttpClient();
            }

            return _httpClient;
        }

        private void CreateHttpClient()
        {
            lock (this)
            {
                if (_httpClient == null)
                {
                    _messageHandler = CreateMessageHandler(this.Settings);
                    _httpClient = CreateHttpClient(this.Settings, _messageHandler);
                }
            }
        }

        private void DisposeHttpClient()
        {
            lock (this)
            {
                if (_messageHandler != null)
                {
                    _messageHandler.Dispose();
                    _messageHandler = null;
                }

                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }
        }

        internal static Session FromSettings(ODataClientSettings settings)
        {
            return new Session(settings);
        }

        internal static Session FromMetadata(Uri baseUri, string metadataString)
        {
            return new Session(baseUri, metadataString);
        }

        private static HttpClient CreateHttpClient(ODataClientSettings settings, HttpMessageHandler messageHandler)
        {
            if (settings.RequestTimeout >= TimeSpan.FromMilliseconds(1))
            {
                return new HttpClient(messageHandler)
                {
                    Timeout = settings.RequestTimeout,
                };
            }
            else
            {
                return new HttpClient(messageHandler);
            }
        }

        private static HttpMessageHandler CreateMessageHandler(ODataClientSettings settings)
        {
            if (settings.OnCreateMessageHandler != null)
            {
                return settings.OnCreateMessageHandler();
            }
            else
            {
                var clientHandler = new HttpClientHandler();

                // Perform this test to prevent failure to access Credentials/PreAuthenticate properties on SL5
                if (settings.Credentials != null)
                {
                    clientHandler.Credentials = settings.Credentials;
                    if (clientHandler.SupportsPreAuthenticate())
                        clientHandler.PreAuthenticate = true;
                }

                if (settings.OnApplyClientHandler != null)
                {
                    settings.OnApplyClientHandler(clientHandler);
                }

                return clientHandler;
            }
        }
    }
}
