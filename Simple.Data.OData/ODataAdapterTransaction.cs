using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    class ODataAdapterTransaction : IAdapterTransaction
    {
        private readonly ODataTableAdapter _adapter;
        private readonly ODataBatch _batch;

        public ODataAdapterTransaction(ODataTableAdapter adapter)
        {
            _adapter = adapter;
            var clientSettings = new ODataClientSettings
                               {
                                   UrlBase = _adapter.UrlBase,
                                   Credentials = _adapter.Credentials
                               };
            _batch = new ODataBatch(clientSettings);
        }

        public string Name
        {
            get { return null; }
        }

        public ODataBatch Batch
        {
            get { return _batch; }
        }

        public void Commit()
        {
            _batch.Complete();
        }

        public void Rollback()
        {
            _batch.Cancel();
        }

        public void Dispose()
        {
            _batch.Dispose();
        }
    }
}
