using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.FindEntries(command, scalarResult, setTotalCount, out totalCount);
        }

        private int IterateEntries(string collection, string commandText, IDictionary<string, object> entryData,
            Func<string, IDictionary<string, object>, IDictionary<string, object>, int> func)
        {
            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                return func(collection, entryKey, entryData);
            }
            else
            {
                var entries = new ODataClient(_settings).FindEntries(commandText);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    foreach (var entry in entryList)
                    {
                        func(collection, entry, entryData);
                    }
                    return entryList.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int UpdateEntryPropertiesAndAssociations(
            string collection,
            IDictionary<string, object> entryKey,
            IDictionary<string, object> entryData,
            EntryMembers entryMembers)
        {
            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = new FluentClient<IDictionary<string, object>>(this, _schema).For(_schema.FindBaseTable(collection).ActualName).Key(entryKey).CommandText;

            var feedWriter = new ODataFeedWriter();
            var table = _schema.FindConcreteTable(collection);
            var entryElement = feedWriter.CreateDataElement(_schema.TypesNamespace, table.EntityType.Name, entryMembers.Properties);
            var unlinkAssociationNames = new List<string>();
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var association = table.FindAssociation(associatedData.Key);
                if (associatedData.Value != null)
                {
                    CreateLinkElement(entryElement, collection, associatedData);
                }
                else
                {
                    unlinkAssociationNames.Add(association.ActualName);
                }
            }

            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, entryData, entryElement.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntry(command);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(collection, associatedData.Key,
                    feedWriter.CreateLinkPath(command.ContentId),
                    feedWriter.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntry(linkCommand);
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                UnlinkEntry(collection, entryKey, associationName);
            }

            return result;
        }

        private HttpCommand CreateLinkCommand(string collection, string associationName, string entryPath, string linkPath)
        {
            var feedWriter = new ODataFeedWriter();
            var linkEntry = feedWriter.CreateLinkElement(linkPath);
            var linkMethod = _schema.FindAssociation(collection, associationName).IsMultiple ?
                RestVerbs.POST :
                RestVerbs.PUT;

            var commandText = feedWriter.CreateLinkCommand(entryPath, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private HttpCommand CreateUnlinkCommand(string collection, string associationName, string entryPath)
        {
            var commandText = new ODataFeedWriter().CreateLinkCommand(entryPath, associationName);
            return HttpCommand.Delete(commandText);
        }

        private void CreateLinkElement(XElement entry, string collection, KeyValuePair<string, object> associatedData)
        {
            if (associatedData.Value == null)
                return;

            var association = _schema.FindAssociation(collection, associatedData.Key);
            var associatedKeyValues = GetLinkedEntryKeyValues(association.ReferenceTableName, associatedData);
            if (associatedKeyValues != null)
            {
                new ODataFeedWriter().AddDataLink(entry, association.ActualName, association.ReferenceTableName, associatedKeyValues);
            }
        }

        private IEnumerable<object> GetLinkedEntryKeyValues(string collection, KeyValuePair<string, object> entryData)
        {
            var entryProperties = GetLinkedEntryProperties(entryData.Value);
            var associatedKeyNames = _schema.FindConcreteTable(collection).GetKeyNames();
            var associatedKeyValues = new object[associatedKeyNames.Count()];
            for (int index = 0; index < associatedKeyNames.Count(); index++)
            {
                bool ok = entryProperties.TryGetValue(associatedKeyNames[index], out associatedKeyValues[index]);
                if (!ok)
                    return null;
            }
            return associatedKeyValues;
        }

        private IDictionary<string, object> GetLinkedEntryProperties(object entryData)
        {
            if (entryData is ODataEntry)
                return (Dictionary<string, object>)(entryData as ODataEntry);

            var entryProperties = entryData as IDictionary<string, object>;
            if (entryProperties == null)
            {
                entryProperties = new Dictionary<string, object>();
                var entryType = entryData.GetType();
                foreach (var entryProperty in entryType.GetDeclaredProperties())
                {
                    entryProperties.Add(
                        entryProperty.Name,
                        entryType.GetDeclaredProperty(entryProperty.Name).GetValue(entryData, null));
                }
            }
            return entryProperties;
        }

        private EntryMembers ParseEntryMembers(Table table, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            foreach (var item in entryData)
            {
                ParseEntryMember(table, item, entryMembers);
            }

            return entryMembers;
        }

        private void ParseEntryMember(Table table, KeyValuePair<string, object> item, EntryMembers entryMembers)
        {
            if (table.HasColumn(item.Key))
            {
                entryMembers.AddProperty(item.Key, item.Value);
            }
            else if (table.HasAssociation(item.Key))
            {
                var association = table.FindAssociation(item.Key);
                if (association.IsMultiple)
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

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var table = _schema.FindConcreteTable(collection);
            return table.Columns.Any(x => !entryData.ContainsKey(x.ActualName));
        }

        private void RemoveSystemProperties(IDictionary<string, object> entryData)
        {
            if (_settings.IncludeResourceTypeInEntryProperties && entryData.ContainsKey(ODataCommand.ResourceTypeLiteral))
            {
                entryData.Remove(ODataCommand.ResourceTypeLiteral);
            }
        }

        private IDictionary<string, object> ExtractKeyFromCommandText(string collection, string commandText)
        {
            // TODO
            return null;
        }
    }
}
