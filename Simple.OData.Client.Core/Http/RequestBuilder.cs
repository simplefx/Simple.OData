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
        private readonly Session _session;
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

        public RequestBuilder(Session session, bool isBatch = false)
        {
            _session = session;
            if (isBatch)
            {
                _lazyBatchWriter = new Lazy<IBatchWriter>(() => _session.Provider.GetBatchWriter());
            }
        }

        public Task<ODataRequest> CreateGetRequestAsync(string commandText, bool scalarResult = false)
        {
            var request = new ODataRequest(RestVerbs.GET, _session, commandText);
            request.ReturnsScalarResult = scalarResult;
            return Utils.GetTaskFromResult(request);
        }

        public async Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);

            var entryDetails = ParseEntryDetails(entitySet, entryData);
            var entryContent = await CreateEntryAsync(
                RestVerbs.POST,
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryDetails.Properties,
                entryDetails.Links);

            var request = new ODataRequest(RestVerbs.POST, _session, _session.MetadataCache.FindBaseEntitySet(collection).ActualName, entryData, entryContent);
            request.ReturnContent = resultRequired;
            return request;
        }

        public async Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);
            var entryDetails = ParseEntryDetails(entitySet, entryData);

            bool hasPropertiesToUpdate = entryDetails.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);

            var entryContent = await CreateEntryAsync(
                merge ? RestVerbs.MERGE : RestVerbs.PUT,
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryDetails.Properties,
                entryDetails.Links);

            var updateMethod = merge ? RestVerbs.MERGE : RestVerbs.PUT;
            bool checkOptimisticConcurrency = _session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            var request = new ODataRequest(updateMethod, _session, commandText, entryData, entryContent);
            request.ReturnContent = resultRequired;
            request.CheckOptimisticConcurrency = checkOptimisticConcurrency;
            return request;
        }

        public async Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string collection)
        {
            var request = new ODataRequest(RestVerbs.DELETE, _session, commandText);
            request.CheckOptimisticConcurrency = _session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            return request;
        }

        public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryPath, string linkPath)
        {
            var associationName = _session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName);
            var linkContent = await _session.Provider.GetRequestWriter(_lazyBatchWriter).CreateLinkAsync(linkPath);
            var linkMethod = _session.Provider.GetMetadata().IsNavigationPropertyMultiple(collection, associationName) ?
                RestVerbs.POST :
                RestVerbs.PUT;

            var commandText = FormatLinkPath(entryPath, associationName);
            var request = new ODataRequest(linkMethod, _session, commandText, null, linkContent);
            request.IsLink = true;
            return request;
        }

        public Task<ODataRequest> CreateUnlinkRequestAsync(string commandText, string collection, string linkName)
        {
            commandText = FormatLinkPath(commandText, _session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName));
            var request = new ODataRequest(RestVerbs.DELETE, _session, commandText);
            return Utils.GetTaskFromResult(request);
        }

        public Task<ODataRequest> CreateBatchRequestAsync(HttpRequestMessage requestMessage)
        {
            var request = new ODataRequest(RestVerbs.POST, _session, ODataLiteral.Batch, requestMessage);
            return Utils.GetTaskFromResult(request);
        }

        public Task<HttpRequestMessage> CompleteBatchAsync()
        {
            return _lazyBatchWriter.Value.EndBatchAsync();
        }

        //public Task<ODataRequest> CreateLinkCommandAsync(string collection, string associationName, int contentId, int associationId)
        //{
        //    return CreateLinkCommandAsync(collection, associationName, FormatLinkPath(contentId), FormatLinkPath(associationId));
        //}

        //public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, IDictionary<string, object> linkData, bool resultRequired)
        //{
        //    var commandWriter = new CommandWriter(_session, this);

        //    var command = await commandWriter.CreateLinkCommandAsync(collection, linkName, 0, linkData);
        //    request = _requestBuilder.CreateRequest(linkCommand, resultRequired);
        //}

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);
            return entitySet.Metadata.GetStructuralPropertyNames(entitySet.ActualName)
                .Any(x => !entryData.ContainsKey(x));
        }

        private string FormatLinkPath(int contentId)
        {
            return "$" + contentId;
        }

        private string FormatLinkPath(string entryPath, string linkName)
        {
            return string.Format("{0}/$links/{1}", entryPath, linkName);
        }

        public async Task<Stream> CreateEntryAsync(string method, string entityTypeNamespace, string entityTypeName,
            IDictionary<string, object> properties,
            IEnumerable<ReferenceLink> associations)
        {
            var entryContent = await _session.Provider.GetRequestWriter(_lazyBatchWriter).CreateEntryAsync(
                method,
                entityTypeNamespace, entityTypeName,
                properties,
                associations);

            return entryContent;
        }

        public static EntryDetails ParseEntryDetails(EntitySet entitySet, IDictionary<string, object> entryData)
        {
            var entryDetails = new EntryDetails();

            foreach (var item in entryData)
            {
                ParseEntryDetail(entitySet, item, entryDetails);
            }

            return entryDetails;
        }

        private static void ParseEntryDetail(EntitySet entitySet, KeyValuePair<string, object> item, EntryDetails entryDetails)
        {
            if (entitySet.Metadata.HasStructuralProperty(entitySet.ActualName, item.Key))
            {
                entryDetails.AddProperty(item.Key, item.Value);
            }
            else if (entitySet.Metadata.HasNavigationProperty(entitySet.ActualName, item.Key))
            {
                if (entitySet.Metadata.IsNavigationPropertyMultiple(entitySet.ActualName, item.Key))
                {
                    var collection = item.Value as IEnumerable<object>;
                    if (collection != null)
                    {
                        foreach (var element in collection)
                        {
                            AddEntryLink(entryDetails, item.Key, element);
                        }
                    }
                }
                else
                {
                    AddEntryLink(entryDetails, item.Key, item.Value);
                }
            }
            else
            {
                throw new UnresolvableObjectException(item.Key, string.Format("No property or association found for {0}.", item.Key));
            }
        }

        private static void AddEntryLink(EntryDetails entryDetails, string associationName, object associatedData)
        {
            string contentId = null;
            entryDetails.AddLink(associationName, associatedData, contentId);
        }

        //public void AddLink(CommandContent content, string collection, KeyValuePair<string, object> associatedData)
        //{
        //    if (associatedData.Value == null)
        //        return;

        //    var associatedKeyValues = GetLinkedEntryKeyValues(
        //        _session.ProviderMetadata.GetNavigationPropertyPartnerName(collection, associatedData.Key), 
        //        associatedData);
        //    if (associatedKeyValues != null)
        //    {
        //        throw new NotImplementedException();
        //        //AddDataLink(content.Entry,
        //        //    _session.ProviderMetadata.GetNavigationPropertyExactName(collection, associatedData.Key),
        //        //    _session.ProviderMetadata.GetNavigationPropertyPartnerName(collection, associatedData.Key), 
        //        //    associatedKeyValues);
        //    }
        //}

        //private IEnumerable<object> GetLinkedEntryKeyValues(string collection, KeyValuePair<string, object> entryData)
        //{
        //    var entryProperties = GetLinkedEntryProperties(entryData.Value);
        //    var associatedKeyNames = _session.MetadataCache.FindConcreteEntitySet(collection).GetKeyNames();
        //    var associatedKeyValues = new object[associatedKeyNames.Count()];
        //    for (int index = 0; index < associatedKeyNames.Count(); index++)
        //    {
        //        bool ok = entryProperties.TryGetValue(associatedKeyNames[index], out associatedKeyValues[index]);
        //        if (!ok)
        //            return null;
        //    }
        //    return associatedKeyValues;
        //}

        //private IDictionary<string, object> GetLinkedEntryProperties(object entryData)
        //{
        //    if (entryData is ODataEntry)
        //        return (Dictionary<string, object>)(entryData as ODataEntry);

        //    var entryProperties = entryData as IDictionary<string, object>;
        //    if (entryProperties == null)
        //    {
        //        var entryType = entryData.GetType();
        //        entryProperties = Utils.GetMappedProperties(entryType).ToDictionary
        //        (
        //            x => x.GetMappedName(),
        //            x => Utils.GetMappedProperty(entryType, x.Name).GetValue(entryData, null)
        //        );
        //    }
        //    return entryProperties;
        //}
    }
}
