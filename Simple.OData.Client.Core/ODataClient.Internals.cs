using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private async Task<IEnumerable<IDictionary<string, object>>> RetrieveEntriesAsync(
            string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            var command = new CommandWriter(_schema).CreateGetCommand(commandText, scalarResult);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.FindEntriesAsync(request, scalarResult, cancellationToken);
        }

        private async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> RetrieveEntriesWithCountAsync(
            string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            var command = new CommandWriter(_schema).CreateGetCommand(commandText, scalarResult);
            var request = _requestBuilder.CreateRequest(command);
            var result = await _requestRunner.FindEntriesWithCountAsync(request, scalarResult, cancellationToken);
            return Tuple.Create(result.Item1, result.Item2);
        }

        private async Task<IEnumerable<IDictionary<string, object>>> IterateEntriesAsync(
            string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired,
            Func<string, IDictionary<string, object>, IDictionary<string, object>, bool, Task<IDictionary<string, object>>> funcAsync, CancellationToken cancellationToken)
        {
            IEnumerable<IDictionary<string, object>> result = null;

            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                result = new [] { await funcAsync(collection, entryKey, entryData, resultRequired) };
            }
            else
            {
                var client = new ODataClient(_settings);
                var entries = await client.FindEntriesAsync(commandText, cancellationToken);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    var resultList = new List<IDictionary<string, object>>();
                    foreach (var entry in entryList)
                    {
                        resultList.Add(await funcAsync(collection, entry, entryData, resultRequired));
                        if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                    }
                    result = resultList;
                }
            }

            return result;
        }

        private async Task<int> IterateEntriesAsync(
            string collection, string commandText,
            Func<string, IDictionary<string, object>, Task> funcAsync, CancellationToken cancellationToken)
        {
            var result = 0;
            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                await funcAsync(collection, entryKey);
                result = 1;
            }
            else
            {
                var client = new ODataClient(_settings);
                var entries = await client.FindEntriesAsync(commandText, cancellationToken);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    foreach (var entry in entryList)
                    {
                        await funcAsync(collection, entry);
                        if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                        ++result;
                    }
                }
            }
            return result;
        }

        private async Task<IDictionary<string, object>> UpdateEntryPropertiesAndAssociationsAsync(
            string collection,
            IDictionary<string, object> entryKey,
            IDictionary<string, object> entryData,
            EntryMembers entryMembers,
            bool resultRequired, 
            CancellationToken cancellationToken)
        {
            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = await GetFluentClient()
                .For(_schema.FindBaseTable(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandWriter = new CommandWriter(_schema);
            var table = _schema.FindConcreteTable(collection);
            var entryContent = commandWriter.CreateEntry(table.EntityType.Namespace, table.EntityType.Name, entryMembers.Properties);
            var unlinkAssociationNames = new List<string>();
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var associationName = table.Schema.ProviderMetadata.GetNavigationPropertyExactName(table.ActualName, associatedData.Key);
                if (associatedData.Value != null)
                {
                    commandWriter.AddLink(entryContent, collection, associatedData);
                }
                else
                {
                    unlinkAssociationNames.Add(associationName);
                }
            }

            var command = commandWriter.CreateUpdateCommand(commandText, entryData, entryContent, merge);
            var request = _requestBuilder.CreateRequest(command, resultRequired, table.EntityType.CheckOptimisticConcurrency);
            var result = await _requestRunner.UpdateEntryAsync(request, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand);
                await _requestRunner.UpdateEntryAsync(request, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                await UnlinkEntryAsync(collection, entryKey, associationName, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
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
            if (table.Schema.ProviderMetadata.HasStructuralProperty(table.ActualName, item.Key))
            {
                entryMembers.AddProperty(item.Key, item.Value);
            }
            else if (table.Schema.ProviderMetadata.HasNavigationProperty(table.ActualName, item.Key))
            {
                if (table.Schema.ProviderMetadata.IsNavigationPropertyMultiple(table.ActualName, item.Key))
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
            return table.Schema.ProviderMetadata.GetStructuralPropertyNames(table.ActualName)
                .Any(x => !entryData.ContainsKey(x));
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
