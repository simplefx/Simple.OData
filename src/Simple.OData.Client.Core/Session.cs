using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class Session : ISession
    {
        private IODataAdapter _adapter;
        private HttpConnection _httpConnection;
        private EdmMetadataCache _metadataCache;

        private Session(Uri baseUri, string metadataString)
        {
            // Create a local setting with the correct uri and metadata
            Settings = new ODataClientSettings
            {
                BaseUri = baseUri,
                MetadataDocument = metadataString
            };
        }

        private Session(ODataClientSettings settings)
        {
            if (settings.BaseUri == null || string.IsNullOrEmpty(settings.BaseUri.AbsoluteUri))
            {
                throw new InvalidOperationException("Unable to create client session with no URI specified.");
            }

            Settings = settings;
        }

        public EdmMetadataCache MetadataCache
        {
            get
            {
                if (_metadataCache == null)
                {
                    lock (this)
                    {
                        if (_metadataCache == null)
                        {
                            _metadataCache = InitializeMetadataCache().Result;
                        }
                    }
                }

                return _metadataCache;
            } 
        }

        public ODataClientSettings Settings { get; }

        public ITypeCache TypeCache => MetadataCache.TypeCache;

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
            Settings.OnTrace?.Invoke(message, messageParams);
        }

        public void ClearMetadataCache()
        {
            var metadataCache = _metadataCache;
            if (metadataCache != null)
            {
                EdmMetadataCache.Clear(metadataCache.Key);
                _metadataCache = null;
            }
        }

        public Task<IODataAdapter> ResolveAdapterAsync(CancellationToken cancellationToken)
        {
            if (Settings.PayloadFormat == ODataPayloadFormat.Unspecified)
            {
                Settings.PayloadFormat = Adapter.DefaultPayloadFormat;
            }

            return Task.FromResult(Adapter);
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
                        {
                            _adapter = MetadataCache.GetODataAdapter(this);
                        }
                    }
                }
                return _adapter;
            }
        }

        public IMetadata Metadata => Adapter.GetMetadata();

        public HttpConnection GetHttpConnection()
        {
            if (_httpConnection == null)
            {
                lock (this)
                {
                    if (_httpConnection == null)
                    {
                        _httpConnection = new HttpConnection(Settings);
                    }
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

        private async Task<EdmMetadataCache> InitializeMetadataCache()
        {
            return await EdmMetadataCache.GetOrAddAsync(
                Settings.BaseUri.AbsoluteUri,
                async uri =>
                {
                    var metadata = await ResolveMetadataAsync(new CancellationToken()).ConfigureAwait(false);
                    return CreateMdc(uri, metadata);
                });
        }

        private async Task<string> ResolveMetadataAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(Settings.MetadataDocument))
            {
                return Settings.MetadataDocument;
            }

            var response = await SendMetadataRequestAsync(cancellationToken).ConfigureAwait(false);
            var metadataDocument = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return metadataDocument;
        }

        private EdmMetadataCache CreateMdc(string key, string metadata)
        {
            // TODO: Here we can change from static to typeCache, conditional on Settings
            return new EdmMetadataCache(key, metadata, new TypeCache());
        }
    }
}
