using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    class ODataAdapterTransaction : IAdapterTransaction
    {
        private readonly ODataTableAdapter _adapter;
        private readonly BatchRequestBuilder _requestBuilder;
        private readonly BatchRequestRunner _requestRunner;
        private bool _active;

        public ODataAdapterTransaction(ODataTableAdapter adapter)
        {
            _adapter = adapter;
            _requestBuilder = new BatchRequestBuilder(_adapter.UrlBase);
            _requestRunner = new BatchRequestRunner(_requestBuilder);

            _requestBuilder.BeginBatch();
            _active = true;
        }

        public string Name
        {
            get { return null; }
        }

        public RequestBuilder RequestBuilder
        {
            get { return _requestBuilder; }
        }

        public RequestRunner RequestRunner
        {
            get { return _requestRunner; }
        }

        public bool IsActive
        {
            get { return _active; }
        }

        public void Commit()
        {
            _requestBuilder.EndBatch();
            using (var response = _requestRunner.TryRequest(_requestBuilder.Request))
            {
            }

            _active = false;
        }

        public void Rollback()
        {
            _requestBuilder.CancelBatch();
            _active = false;
        }

        public void Dispose()
        {
            _active = false;
        }
    }
}
