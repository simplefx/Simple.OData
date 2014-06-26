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
            _batch = new ODataBatch(_adapter.ClientSettings);
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
            Utils.ExecuteAndUnwrap(() => _batch.CompleteAsync());
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
