using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public abstract class RequestWriterBase : IRequestWriter
    {
        protected readonly ISession _session;
        protected readonly Lazy<IBatchWriter> _deferredBatchWriter;

        protected RequestWriterBase(ISession session, Lazy<IBatchWriter> deferredBatchWriter)
        {
            _session = session;
            _deferredBatchWriter = deferredBatchWriter;
        }

        protected bool IsBatch 
        {
            get { return _deferredBatchWriter != null; }
        }

        public async Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entityCollectionName = _session.Metadata.GetBaseEntityCollection(collection).ActualName;
            var entryContent = await WriteEntryContentAsync(
                RestVerbs.Post, collection, entryData, entityCollectionName);

            var request = new ODataRequest(RestVerbs.Post, _session, entityCollectionName, entryData, entryContent)
            {
                ReturnContent = resultRequired,
            };
            return request;
        }

        public async Task<ODataRequest> CreateUpdateRequestAsync(string collection, string entryIdent, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            var entryDetails = _session.Metadata.ParseEntryDetails(entityCollection.ActualName, entryData);

            var hasPropertiesToUpdate = entryDetails.Properties.Count > 0;
            var merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);

            var entryContent = await WriteEntryContentAsync(
                merge ? RestVerbs.Patch : RestVerbs.Put, collection, entryData, entryIdent);

            var updateMethod = merge ? RestVerbs.Patch : RestVerbs.Put;
            var checkOptimisticConcurrency = _session.Metadata.EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            var request = new ODataRequest(updateMethod, _session, entryIdent, entryData, entryContent)
            {
                ReturnContent = resultRequired,
                CheckOptimisticConcurrency = checkOptimisticConcurrency
            };
            return request;
        }

        public async Task<ODataRequest> CreateDeleteRequestAsync(string collection, string entryIdent)
        {
            await WriteEntryContentAsync(
                RestVerbs.Delete, collection, null, entryIdent);

            var request = new ODataRequest(RestVerbs.Delete, _session, entryIdent)
            {
                CheckOptimisticConcurrency =
                    _session.Metadata.EntitySetTypeRequiresOptimisticConcurrencyCheck(collection)
            };
            return request;
        }

        public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryIdent, string linkIdent)
        {
            var associationName = _session.Metadata.GetNavigationPropertyExactName(collection, linkName);
            var linkMethod = _session.Metadata.IsNavigationPropertyMultiple(collection, associationName) ?
                RestVerbs.Post :
                RestVerbs.Put;

            var linkContent = await WriteLinkContentAsync(linkIdent);
            var commandText = FormatLinkPath(entryIdent, associationName);
            var request = new ODataRequest(linkMethod, _session, commandText, null, linkContent)
            {
                IsLink = true,
            };
            return request;
        }

        public async Task<ODataRequest> CreateUnlinkRequestAsync(string collection, string linkName, string entryIdent)
        {
            await WriteEntryContentAsync(RestVerbs.Delete, collection, null, entryIdent);

            entryIdent = FormatLinkPath(entryIdent, _session.Metadata.GetNavigationPropertyExactName(collection, linkName));
            var request = new ODataRequest(RestVerbs.Delete, _session, entryIdent)
            {
                IsLink = true,
            };
            return request;
        }

        protected abstract Task<Stream> WriteEntryContentAsync(string method, string collection, IDictionary<string, object> entryData, string commandText);
        protected abstract Task<Stream> WriteLinkContentAsync(string linkKey);
        protected abstract string FormatLinkPath(string entryKey, string navigationPropertyName);

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            return _session.Metadata.GetStructuralPropertyNames(entityCollection.ActualName)
                .Any(x => !entryData.ContainsKey(x));
        }
    }
}