using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private async Task<IEnumerable<IDictionary<string, object>>> RetrieveEntriesAsync(string commandText, bool scalarResult)
        {
            var command = new CommandWriter(_schema).CreateGetCommand(commandText, scalarResult);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.FindEntriesAsync(request, scalarResult);
        }

        private async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> RetrieveEntriesWithCountAsync(string commandText, bool scalarResult)
        {
            var command = new CommandWriter(_schema).CreateGetCommand(commandText, scalarResult);
            var request = _requestBuilder.CreateRequest(command);
            var result = await _requestRunner.FindEntriesWithCountAsync(request, scalarResult);
            return Tuple.Create(result.Item1, result.Item2);
        }

        private async Task<int> IterateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData,
            Func<string, IDictionary<string, object>, IDictionary<string, object>, Task> func)
        {
            var result = 0;

            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                await func(collection, entryKey, entryData);
                result = 1;
            }
            else
            {
                var client = new ODataClient(_settings);
                var entries = await client.FindEntriesAsync(commandText);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    foreach (var entry in entryList)
                    {
                        await func(collection, entry, entryData);
                    }
                    result = entryList.Count;
                }
            }

            return result;
        }

        private async Task UpdateEntryPropertiesAndAssociationsAsync(
            string collection,
            IDictionary<string, object> entryKey,
            IDictionary<string, object> entryData,
            EntryMembers entryMembers)
        {
            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = await GetFluentClient()
                .For(_schema.FindBaseTable(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync();

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
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.UpdateEntryAsync(request);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand);
                await _requestRunner.UpdateEntryAsync(request);
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                await UnlinkEntryAsync(collection, entryKey, associationName);
            }
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
