using System;

namespace Simple.OData.Client
{
    public class ODataBatch : IDisposable 
    {
        internal BatchRequestBuilder RequestBuilder { get; set; }
        internal BatchRequestRunner RequestRunner { get; set; }
        private bool _active;

        public ODataBatch(string urlBase)
        {
            this.RequestBuilder = new BatchRequestBuilder(urlBase);
            this.RequestRunner = new BatchRequestRunner(this.RequestBuilder);

            this.RequestBuilder.BeginBatch();
            _active = true;
        }

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
