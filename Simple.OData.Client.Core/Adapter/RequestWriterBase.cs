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
            var commandText = collection;
            var segments = collection.Split('/');
            if (segments.Count() > 1 && segments.Last().Contains("."))
            {
                commandText = collection.Substring(0, collection.Length - segments.Last().Length - 1);
            }

            var entryContent = await WriteEntryContentAsync(
                RestVerbs.Post, collection, commandText, entryData);

            var request = new ODataRequest(RestVerbs.Post, _session, commandText, entryData, entryContent)
            {
                ResultRequired = resultRequired,
            };
            AssignHeaders(request);
            return request;
        }

        public async Task<ODataRequest> CreateUpdateRequestAsync(string collection, string entryIdent, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            var entryDetails = _session.Metadata.ParseEntryDetails(entityCollection.ActualName, entryData);

            var hasPropertiesToUpdate = entryDetails.Properties.Count > 0;
            var merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);

            var entryContent = await WriteEntryContentAsync(
                merge ? RestVerbs.Patch : RestVerbs.Put, collection, entryIdent, entryData);

            var updateMethod = merge ? RestVerbs.Patch : RestVerbs.Put;
            var checkOptimisticConcurrency = _session.Metadata.EntityCollectionTypeRequiresOptimisticConcurrencyCheck(collection);
            var request = new ODataRequest(updateMethod, _session, entryIdent, entryData, entryContent)
            {
                ResultRequired = resultRequired,
                CheckOptimisticConcurrency = checkOptimisticConcurrency
            };
            AssignHeaders(request);
            return request;
        }

        public async Task<ODataRequest> CreateDeleteRequestAsync(string collection, string entryIdent)
        {
            await WriteEntryContentAsync(
                RestVerbs.Delete, collection, entryIdent, null);

            var request = new ODataRequest(RestVerbs.Delete, _session, entryIdent)
            {
                CheckOptimisticConcurrency = _session.Metadata.EntityCollectionTypeRequiresOptimisticConcurrencyCheck(collection)
            };
            AssignHeaders(request);
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
            AssignHeaders(request);
            return request;
        }

        public async Task<ODataRequest> CreateUnlinkRequestAsync(string collection, string linkName, string entryIdent, string linkIdent)
        {
            var associationName = _session.Metadata.GetNavigationPropertyExactName(collection, linkName);
            await WriteEntryContentAsync(RestVerbs.Delete, collection, entryIdent, null);

            var commandText = FormatLinkPath(entryIdent, associationName, linkIdent);
            var request = new ODataRequest(RestVerbs.Delete, _session, commandText)
            {
                IsLink = true,
            };
            AssignHeaders(request);
            return request;
        }

        protected abstract Task<Stream> WriteEntryContentAsync(string method, string collection, string commandText, IDictionary<string, object> entryData);
        protected abstract Task<Stream> WriteLinkContentAsync(string linkIdent);
        protected abstract string FormatLinkPath(string entryIdent, string navigationPropertyName, string linkIdent = null);
        protected abstract void AssignHeaders(ODataRequest request);

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            return _session.Metadata.GetStructuralPropertyNames(entityCollection.ActualName)
                .Any(x => !entryData.ContainsKey(x));
        }
    }
}