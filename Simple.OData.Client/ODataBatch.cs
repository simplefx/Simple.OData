using System;

namespace Simple.OData.Client
{
    public class ODataBatch : IDisposable 
    {
        internal BatchRequestBuilder RequestBuilder { get; set; }
        internal BatchRequestRunner RequestRunner { get; set; }
        private bool _active;

#if (NET20 || NET35 || NET40 || SILVERLIGHT)
        public ODataBatch(string urlBase)
            : this(urlBase, new Credentials(null, null, null, false))
        {
        }

        public ODataBatch(string urlBase, string user, string password, string domain = null)
            : this(urlBase, new Credentials(user, password, domain, false))
        {
        }

        public ODataBatch(string urlBase, bool integratedSecurity)
            : this(urlBase, new Credentials(null, null, null, integratedSecurity))
        {
        }

        public ODataBatch(string urlBase, Credentials credentials)
        {
            this.RequestBuilder = new BatchRequestBuilder(urlBase, credentials);
            this.RequestRunner = new BatchRequestRunner(this.RequestBuilder);

            this.RequestBuilder.BeginBatch();
            _active = true;
        }
#else
        public ODataBatch(string urlBase)
        {
            this.RequestBuilder = new BatchRequestBuilder(urlBase);
            this.RequestRunner = new BatchRequestRunner(this.RequestBuilder);

            this.RequestBuilder.BeginBatch();
            _active = true;
        }
#endif

        public void Dispose()
        {
            if (_active)
                this.RequestBuilder.CancelBatch();
            _active = false;
        }

        public void Complete()
        {
            this.RequestBuilder.EndBatch();
            using (var response = this.RequestRunner.TryRequest(this.RequestBuilder.Request))
            {
            }
            _active = false;
        }

        public void Cancel()
        {
            this.RequestBuilder.CancelBatch();
            _active = false;
        }
    }
}
