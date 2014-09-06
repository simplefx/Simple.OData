using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class CommandWriter
    {
        private readonly Session _session;
        private readonly RequestBuilder _requestBuilder;

        public CommandWriter(Session session, RequestBuilder requestBuilder)
        {
            _session = session;
            _requestBuilder = requestBuilder;
        }

        public async Task<HttpCommand> CreateGetCommandAsync(string commandText, bool scalarResult = false)
        {
            return HttpCommand.Get(commandText, scalarResult);
        }

        public async Task<HttpCommand> CreateInsertCommandAsync(string commandText, IDictionary<string, object> entryData, string collection, EntitySet entitySet)
        {
            var entryMembers = ParseEntryMembers(entitySet, entryData);
            var entryContent = await CreateEntryAsync(
                RestVerbs.POST,
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            return HttpCommand.Post(commandText, entryData, entryContent);
        }

        public async Task<HttpCommand> CreateUpdateCommandAsync(string commandText, IDictionary<string, object> entryData, string collection, EntitySet entitySet, bool merge = false)
        {
            var entryMembers = ParseEntryMembers(entitySet, entryData);
            var entryContent = await CreateEntryAsync(
                merge ? RestVerbs.MERGE : RestVerbs.PUT, 
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            return new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, entryData, entryContent);
        }

        public async Task<HttpCommand> CreateDeleteCommandAsync(string commandText)
        {
            return HttpCommand.Delete(commandText);
        }

        public Task<HttpCommand> CreateLinkCommandAsync(string collection, string associationName, int contentId, int associationId)
        {
            return CreateLinkCommandAsync(collection, associationName, FormatLinkPath(contentId), FormatLinkPath(associationId));
        }

        public async Task<HttpCommand> CreateLinkCommandAsync(string collection, string associationName, string entryPath, string linkPath)
        {
            var linkEntry = await CreateLinkAsync(linkPath);
            var linkMethod = _session.Provider.GetMetadata().IsNavigationPropertyMultiple(collection, associationName) ?
                RestVerbs.POST :
                RestVerbs.PUT;

            var commandText = FormatLinkPath(entryPath, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        public async Task<HttpCommand> CreateUnlinkCommandAsync(string collection, string associationName, string entryPath)
        {
            var commandText = FormatLinkPath(entryPath, associationName);
            return HttpCommand.Delete(commandText);
        }

        public async Task<string> CreateEntryAsync(string method, string entityTypeNamespace, string entityTypeName,
            IDictionary<string, object> properties, 
            IEnumerable<KeyValuePair<string, object>> associationsByValue,
            IEnumerable<KeyValuePair<string, int>> associationsByContentId)
        {
            var entry = await _session.Provider.GetRequestWriter().CreateEntryAsync(
                method,
                entityTypeNamespace, entityTypeName, 
                properties, 
                associationsByValue, 
                associationsByContentId);

            return entry;
        }

        public Task<string> CreateLinkAsync(string linkPath)
        {
            return _session.Provider.GetRequestWriter().CreateLinkAsync(linkPath);
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

        public EntryMembers ParseEntryMembers(EntitySet entitySet, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            foreach (var item in entryData)
            {
                ParseEntryMember(entitySet, item, entryMembers);
            }

            return entryMembers;
        }

        private void ParseEntryMember(EntitySet entitySet, KeyValuePair<string, object> item, EntryMembers entryMembers)
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

        private void AddEntryAssociation(EntryMembers entryMembers, string associationName, object associatedData)
        {
            int contentId = _requestBuilder.GetContentId(associatedData);
            if (contentId == 0)
            {
                entryMembers.AddAssociationByValue(associationName, associatedData);
            }
            else
            {
                entryMembers.AddAssociationByContentId(associationName, contentId);
            }
        }

        private string FormatLinkPath(int contentId)
        {
            return "$" + contentId;
        }

        private string FormatLinkPath(string entryPath, string linkName)
        {
            return string.Format("{0}/$links/{1}", entryPath, linkName);
        }

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
