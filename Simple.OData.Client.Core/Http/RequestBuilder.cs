using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestBuilder
    {
        public Session Session { get; private set; }
        public string Host
        {
            get
            {
                if (string.IsNullOrEmpty(this.Session.UrlBase)) return null;
                var substr = this.Session.UrlBase.Substring(this.Session.UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        protected RequestBuilder(Session session)
        {
            this.Session = session;
        }

        public virtual Lazy<IBatchWriter> GetDeferredBatchWriter() { return null; }
        public virtual int GetBatchContentId(object content) { return 0; }

        public async Task<ODataRequest> CreateGetRequestAsync(string commandText, bool scalarResult = false)
        {
            var request = new ODataRequest(RestVerbs.GET, this.Session, commandText);
            request.ReturnsScalarResult = scalarResult;
            return request;
        }

        public async Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entitySet = this.Session.MetadataCache.FindConcreteEntitySet(collection);

            var entryMembers = ParseEntryMembers(entitySet, entryData);
            var entryContent = await CreateEntryAsync(
                RestVerbs.POST,
                this.Session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                this.Session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            var request = new ODataRequest(RestVerbs.POST, this.Session, this.Session.MetadataCache.FindBaseEntitySet(collection).ActualName, entryData, entryContent);
            request.ReturnContent = resultRequired;
            return request;
        }

        public async Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entitySet = this.Session.MetadataCache.FindConcreteEntitySet(collection);
            var entryMembers = ParseEntryMembers(entitySet, entryData);

            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);

            var entryContent = await CreateEntryAsync(
                merge ? RestVerbs.MERGE : RestVerbs.PUT,
                this.Session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                this.Session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            var updateMethod = merge ? RestVerbs.MERGE : RestVerbs.PUT;
            bool checkOptimisticConcurrency = this.Session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            var request = new ODataRequest(updateMethod, this.Session, commandText, entryData, entryContent);
            request.ReturnContent = resultRequired;
            request.CheckOptimisticConcurrency = checkOptimisticConcurrency;
            return request;
        }

        public async Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string collection)
        {
            var request = new ODataRequest(RestVerbs.DELETE, this.Session, commandText);
            request.CheckOptimisticConcurrency = this.Session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection);
            return request;
        }

        public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryPath, string linkPath)
        {
            var associationName = this.Session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName);
            var linkEntry = await CreateLinkAsync(linkPath);
            var linkMethod = this.Session.Provider.GetMetadata().IsNavigationPropertyMultiple(collection, associationName) ?
                RestVerbs.POST :
                RestVerbs.PUT;

            var commandText = FormatLinkPath(entryPath, associationName);
            var request = new ODataRequest(linkMethod, this.Session, commandText, null, linkEntry);
            request.IsLink = true;
            return request;
        }

        public async Task<ODataRequest> CreateUnlinkRequestAsync(string commandText, string collection, string linkName)
        {
            commandText = FormatLinkPath(commandText, this.Session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName));
            return new ODataRequest(RestVerbs.DELETE, this.Session, commandText);
        }

        private Task<string> CreateLinkAsync(string linkPath)
        {
            return this.Session.Provider.GetRequestWriter(GetDeferredBatchWriter()).CreateLinkAsync(linkPath);
        }

        //public Task<ODataRequest> CreateLinkCommandAsync(string collection, string associationName, int contentId, int associationId)
        //{
        //    return CreateLinkCommandAsync(collection, associationName, FormatLinkPath(contentId), FormatLinkPath(associationId));
        //}

        //public async Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, IDictionary<string, object> linkData, bool resultRequired)
        //{
        //    var commandWriter = new CommandWriter(this.Session, this);

        //    var command = await commandWriter.CreateLinkCommandAsync(collection, linkName, 0, linkData);
        //    request = _requestBuilder.CreateRequest(linkCommand, resultRequired);
        //}

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entitySet = this.Session.MetadataCache.FindConcreteEntitySet(collection);
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

        public async Task<string> CreateEntryAsync(string method, string entityTypeNamespace, string entityTypeName,
            IDictionary<string, object> properties,
            IEnumerable<KeyValuePair<string, object>> associationsByValue,
            IEnumerable<KeyValuePair<string, int>> associationsByContentId)
        {
            var entry = await this.Session.Provider.GetRequestWriter(GetDeferredBatchWriter()).CreateEntryAsync(
                method,
                entityTypeNamespace, entityTypeName,
                properties,
                associationsByValue,
                associationsByContentId);

            return entry;
        }

        public static EntryMembers ParseEntryMembers(EntitySet entitySet, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            foreach (var item in entryData)
            {
                ParseEntryMember(entitySet, item, entryMembers);
            }

            return entryMembers;
        }

        private static void ParseEntryMember(EntitySet entitySet, KeyValuePair<string, object> item, EntryMembers entryMembers)
        {
            if (entitySet.Metadata.HasStructuralProperty(entitySet.ActualName, item.Key))
            {
                entryMembers.AddProperty(item.Key, item.Value);
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
                            AddEntryAssociation(entryMembers, item.Key, element);
                        }
                    }
                }
                else
                {
                    AddEntryAssociation(entryMembers, item.Key, item.Value);
                }
            }
            else
            {
                throw new UnresolvableObjectException(item.Key, string.Format("No property or association found for {0}.", item.Key));
            }
        }

        private static void AddEntryAssociation(EntryMembers entryMembers, string associationName, object associatedData)
        {
            int contentId = 0;//_requestBuilder.GetBatchContentId(associatedData);
            if (contentId == 0)
            {
                entryMembers.AddAssociationByValue(associationName, associatedData);
            }
            else
            {
                entryMembers.AddAssociationByContentId(associationName, contentId);
            }
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

        //private string GetQualifiedResourceName(string namespaceName, string collectionName)
        //{
        //    return string.Join(".", namespaceName, collectionName);
        //}
    }
}
