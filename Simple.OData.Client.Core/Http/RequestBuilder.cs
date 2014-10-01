using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestBuilder
    {
        private readonly ISession _session;
        private readonly Lazy<IBatchWriter> _lazyBatchWriter;

        public bool IsBatch { get { return _lazyBatchWriter != null; } }
        public string Host
        {
            get
            {
                if (string.IsNullOrEmpty(_session.UrlBase)) return null;
                var substr = _session.UrlBase.Substring(_session.UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

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
            var request = new ODataRequest(RestVerbs.Get, _session, commandText);
            request.ReturnsScalarResult = scalarResult;
            return Utils.GetTaskFromResult(request);
        }

        public async Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entityCollectionName = _session.Metadata.GetBaseEntityCollection(collection).ActualName;
            var entryContent = await _session.Adapter.GetRequestWriter(_lazyBatchWriter).WriteEntryContentAsync(
                RestVerbs.Post, collection, entryData, entityCollectionName);

            var request = new ODataRequest(RestVerbs.Post, _session, 
                entityCollectionName, entryData, entryContent);
            request.ReturnContent = resultRequired;
            return request;
        }

        public async Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            var entryDetails = _session.Metadata.ParseEntryDetails(entityCollection.ActualName, entryData);

            bool hasPropertiesToUpdate = entryDetails.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);

            var entryContent = await _session.Adapter.GetRequestWriter(_lazyBatchWriter).WriteEntryContentAsync(
                merge ? RestVerbs.Patch : RestVerbs.Put, collection, entryData, commandText);

            var updateMethod = merge ? RestVerbs.Patch : RestVerbs.Put;
            bool checkOptimisticConcurrency = _session.Metadata.EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            var request = new ODataRequest(updateMethod, _session, commandText, entryData, entryContent);
            request.ReturnContent = resultRequired;
            request.CheckOptimisticConcurrency = checkOptimisticConcurrency;
            return request;
        }

        public async Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string collection)
        {
            await _session.Adapter.GetRequestWriter(_lazyBatchWriter).WriteEntryContentAsync(
                RestVerbs.Delete, collection, null, commandText);

            var request = new ODataRequest(RestVerbs.Delete, _session, commandText);
            request.CheckOptimisticConcurrency = _session.Metadata.EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            return request;
        }

        public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryPath, string linkPath)
        {
            var associationName = _session.Metadata.GetNavigationPropertyExactName(collection, linkName);
            var linkMethod = _session.Metadata.IsNavigationPropertyMultiple(collection, associationName) ?
                RestVerbs.Post :
                RestVerbs.Put;

            var requestWriter = _session.Adapter.GetRequestWriter(_lazyBatchWriter);
            var linkContent = await requestWriter.WriteLinkContentAsync(linkPath);
            var commandText = requestWriter.FormatLinkPath(entryPath, associationName);
            var request = new ODataRequest(linkMethod, _session, commandText, null, linkContent);
            request.IsLink = true;
            return request;
        }

        public async Task<ODataRequest> CreateUnlinkRequestAsync(string commandText, string collection, string linkName)
        {
            var requestWriter = _session.Adapter.GetRequestWriter(_lazyBatchWriter);
            await requestWriter.WriteEntryContentAsync(RestVerbs.Delete, collection, null, commandText);

            commandText = requestWriter.FormatLinkPath(commandText, _session.Metadata.GetNavigationPropertyExactName(collection, linkName));
            var request = new ODataRequest(RestVerbs.Delete, _session, commandText);
            return await Utils.GetTaskFromResult(request);
        }

        public Task<ODataRequest> CreateBatchRequestAsync(HttpRequestMessage requestMessage)
        {
            var request = new ODataRequest(RestVerbs.Post, _session, ODataLiteral.Batch, requestMessage);
            return Utils.GetTaskFromResult(request);
        }

        public Task<HttpRequestMessage> CompleteBatchAsync()
        {
            return _lazyBatchWriter.Value.EndBatchAsync();
        }

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            return _session.Metadata.GetStructuralPropertyNames(entityCollection.ActualName)
                .Any(x => !entryData.ContainsKey(x));
        }
    }
}
