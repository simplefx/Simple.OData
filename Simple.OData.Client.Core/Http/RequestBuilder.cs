using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestBuilder
    {
        private readonly ISession _session;
        private readonly Lazy<IBatchWriter> _lazyBatchWriter;

        public bool IsBatch { get { return _lazyBatchWriter != null; } }
        public bool IsBatchWithActions { get { return _lazyBatchWriter != null && _lazyBatchWriter.IsValueCreated; } }

        public RequestBuilder(ISession session, bool isBatch = false)
        {
            _session = session;
            if (isBatch)
            {
                _lazyBatchWriter = new Lazy<IBatchWriter>(() => _session.Adapter.GetBatchWriter());
            }
        }

        public Task<ODataRequest> CreateGetRequestAsync(string commandText, bool scalarResult = false)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(commandText, scalarResult);
        }

        public Task<ODataRequest> CreateInsertRequestAsync(string commandText, IDictionary<string, object> entryData, bool resultRequired)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateInsertRequestAsync(commandText, entryData, resultRequired);
        }

        public Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string entryIdent, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateUpdateRequestAsync(commandText, entryIdent, entryKey, entryData, resultRequired);
        }

        public Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string entryIdent)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateDeleteRequestAsync(commandText, entryIdent);
        }

        public Task<ODataRequest> CreateLinkRequestAsync(string commandText, string linkName, string entryIdent, string linkIdent)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateLinkRequestAsync(commandText, linkName, entryIdent, linkIdent);
        }

        public Task<ODataRequest> CreateUnlinkRequestAsync(string commandText, string linkName, string entryIdent, string linkIdent)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateUnlinkRequestAsync(commandText, linkName, entryIdent, linkIdent);
        }

        public Task<ODataRequest> CreateFunctionRequestAsync(string commandText, string functionName)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateFunctionRequestAsync(commandText, functionName);
        }

        public Task<ODataRequest> CreateActionRequestAsync(string commandText, string actionName, IDictionary<string, object> parameters)
        {
            return _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateActionRequestAsync(commandText, actionName, parameters);
        }

        public async Task<ODataRequest> CreateBatchRequestAsync()
        {
            var requestMessage = await _lazyBatchWriter.Value.EndBatchAsync();
            var request = new ODataRequest(RestVerbs.Post, _session, ODataLiteral.Batch, requestMessage);
            return request;
        }
    }
}
