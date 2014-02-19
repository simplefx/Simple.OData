using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            try
            {
                var command = new CommandWriter(_schema).CreateGetCommand(commandText, scalarResult);
                _requestBuilder.AddCommandToRequest(command);
                if (setTotalCount)
                {
                    var result = _requestRunner.FindEntriesWithCountAsync(command, scalarResult).Result;
                    totalCount = result.Item2;
                    return result.Item1;
                }
                else
                {
                    totalCount = 0;
                    return _requestRunner.FindEntriesAsync(command, scalarResult).Result;
                }
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        private int IterateEntries(string collection, string commandText, IDictionary<string, object> entryData,
            Func<string, IDictionary<string, object>, IDictionary<string, object>, int> func)
        {
            try
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
            catch (AggregateException exception)
            {
                throw exception.InnerException;
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
            var commandText = GetFluentClient()
                .For(_schema.FindBaseTable(collection).ActualName)
                .Key(entryKey)
                .CommandText;

            var commandWriter = new CommandWriter(_schema);
            var table = _schema.FindConcreteTable(collection);
            var entryContent = commandWriter.CreateEntry(table.EntityType.Name, entryMembers.Properties);
            var unlinkAssociationNames = new List<string>();
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var association = table.FindAssociation(associatedData.Key);
                if (associatedData.Value != null)
                {
                    commandWriter.AddLink(entryContent, collection, associatedData);
                }
                else
                {
                    unlinkAssociationNames.Add(association.ActualName);
                }
            }

            var command = commandWriter.CreateUpdateCommand(commandText, entryData, entryContent, merge);
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntryAsync(command).Result;

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntryAsync(linkCommand).Wait();
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                UnlinkEntry(collection, entryKey, associationName);
            }

            return result;
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
            if (_settings.IncludeResourceTypeInEntryProperties && entryData.ContainsKey(FluentCommand.ResourceTypeLiteral))
            {
                entryData.Remove(FluentCommand.ResourceTypeLiteral);
            }
        }

        private IDictionary<string, object> ExtractKeyFromCommandText(string collection, string commandText)
        {
            // TODO
            return null;
        }
    }
}
