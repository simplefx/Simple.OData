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
            var command = new CommandWriter(_session, _requestBuilder).CreateGetCommand(commandText, scalarResult);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.FindEntriesAsync(request, scalarResult, cancellationToken);
        }

        private async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> RetrieveEntriesWithCountAsync(
            string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            var command = new CommandWriter(_session, _requestBuilder).CreateGetCommand(commandText, scalarResult);
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

        private async Task<IDictionary<string, object>> InsertEntryAndLinksAsync(string collection, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            RemoveSystemProperties(entryData);
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);

            var commandWriter = new CommandWriter(_session, _requestBuilder);
            var entryMembers = commandWriter.ParseEntryMembers(entitySet, entryData);

            var command = commandWriter.CreateInsertCommand(_session.MetadataCache.FindBaseEntitySet(collection).ActualName, entryData, collection, entitySet);
            var request = _requestBuilder.CreateRequest(command, resultRequired);
            var result = await _requestRunner.InsertEntryAsync(request, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand, resultRequired);
                await _requestRunner.InsertEntryAsync(request, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        public async Task<IDictionary<string, object>> UpdateEntryAndLinksAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);
            var commandWriter = new CommandWriter(_session, _requestBuilder);
            var entryMembers = commandWriter.ParseEntryMembers(entitySet, entryData);

            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = await GetFluentClient()
                .For(_session.MetadataCache.FindBaseEntitySet(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entitySetName = _session.Provider.GetMetadata().GetEntitySetExactName(collection);

            var command = commandWriter.CreateUpdateCommand(commandText, entryData, collection, entitySet, merge);
            var request = _requestBuilder.CreateRequest(command, resultRequired,
                _session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection));
            var result = await _requestRunner.UpdateEntryAsync(request, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand);
                await _requestRunner.UpdateEntryAsync(request, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            var unlinkAssociationNames = entryMembers.AssociationsByValue
                .Where(x => x.Value == null)
                .Select(x => _session.Provider.GetMetadata().GetNavigationPropertyExactName(entitySetName, x.Key))
                .ToList();

            foreach (var associationName in unlinkAssociationNames)
            {
                await UnlinkEntryAsync(collection, entryKey, associationName, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entitySet = _session.MetadataCache.FindConcreteEntitySet(collection);
            return entitySet.Metadata.GetStructuralPropertyNames(entitySet.ActualName)
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
