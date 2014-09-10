using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private async Task<IEnumerable<IDictionary<string, object>>> ExecuteFindEntriesRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (_requestBuilder.IsBatch)
            {
                try
                {
                    using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                    {
                        IEnumerable<IDictionary<string, object>> result = null;
                        if (!response.IsSuccessStatusCode)
                        {
                            result = Enumerable.Empty<IDictionary<string, object>>();
                        }
                        else
                        {
                            var responseReader = _session.Provider.GetResponseReader();
                            var odataResponse = await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties);
                            result = odataResponse.Entries ?? new[] { odataResponse.Entry };
                        }

                        return result;
                    }
                }
                catch (WebRequestException ex)
                {
                    if (_settings.IgnoreResourceNotFoundException && ex.Code == HttpStatusCode.NotFound)
                        return new[] { (IDictionary<string, object>)null };
                    else
                        throw;
                }
            }
            else
            {
                return await Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
            }
        }

        private async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> ExecuteFindEntriesWithCountRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (!_requestBuilder.IsBatch)
            {
                try
                {
                    using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            return Tuple.Create(Enumerable.Empty<IDictionary<string, object>>(), 0);
                        }
                        else
                        {
                            var responseReader = _session.Provider.GetResponseReader();
                            var result =
                                await
                                    responseReader.GetResponseAsync(response,
                                        _settings.IncludeResourceTypeInEntryProperties);
                            return Tuple.Create(result.Entries, (int) result.TotalCount.GetValueOrDefault());
                        }
                    }
                }
                catch (WebRequestException ex)
                {
                    if (_settings.IgnoreResourceNotFoundException && ex.Code == HttpStatusCode.NotFound)
                    {
                        return new Tuple<IEnumerable<IDictionary<string, object>>, int>(
                            new[] {(IDictionary<string, object>) null}, 0);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                return default(Tuple<IEnumerable<IDictionary<string, object>>, int>);
            }
        }

        private async Task<IDictionary<string, object>> ExecuteGetEntryRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (!_requestBuilder.IsBatch)
            {
                try
                {
                    using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                    {
                        var responseReader = _session.Provider.GetResponseReader();
                        return (await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties)).Entry;
                    }
                }
                catch (WebRequestException ex)
                {
                    if (_settings.IgnoreResourceNotFoundException && ex.Code == HttpStatusCode.NotFound)
                        return null;
                    else
                        throw;
                }
            }
            else
            {
                return await Utils.GetTaskFromResult(default(IDictionary<string, object>));
            }
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

            var request = await _requestBuilder.CreateInsertRequestAsync(collection, entryData, resultRequired);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            if (!_requestBuilder.IsBatch)
            {
                using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                    if (request.ReturnContent && response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        var responseReader = _session.Provider.GetResponseReader();
                        return (await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties)).Entry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return await Utils.GetTaskFromResult(request.EntryData);
            }

            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            //var entryMembers = CommandWriter.ParseEntryMembers(entitySet, entryData);
            //foreach (var associatedData in entryMembers.AssociationsByContentId)
            //{
            //    request = await _requestBuilder.CreateLinkRequestAsync(collection, associatedData.Key, associatedData.Value, resultRequired);

            //    var linkCommand = await commandWriter.CreateLinkCommandAsync(collection, associatedData.Key, command.ContentId, associatedData.Value);
            //    request = _requestBuilder.CreateRequest(linkCommand, resultRequired);
            //    await _requestRunner.InsertEntryAsync(request, cancellationToken);
            //    if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            //}

            //return result;
        }

        public async Task<IDictionary<string, object>> UpdateEntryAndLinksAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);

            var commandText = await FormatEntryKeyAsync(collection, entryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateUpdateRequestAsync(commandText, collection, entryKey, entryData, resultRequired);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            IDictionary<string, object> result = null;
            if (!_requestBuilder.IsBatch)
            {
                using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                    if (request.ReturnContent && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var responseReader = _session.Provider.GetResponseReader();
                        result = (await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties)).Entry;
                    }
                }
            }
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            //foreach (var associatedData in entryMembers.AssociationsByContentId)
            //{
            //    var linkCommand = await commandWriter.CreateLinkCommandAsync(collection, associatedData.Key, command.ContentId, associatedData.Value);
            //    request = _requestBuilder.CreateRequest(linkCommand);
            //    await _requestRunner.UpdateEntryAsync(request, cancellationToken);
            //    if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            //}

            var entitySet = this.Session.MetadataCache.FindConcreteEntitySet(collection);
            var entitySetName = this.Session.Provider.GetMetadata().GetEntitySetExactName(collection);
            var entryMembers = RequestBuilder.ParseEntryMembers(entitySet, entryData);
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

        private async Task ExecuteDeleteEntryRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (!_requestBuilder.IsBatch)
            {
                using (await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                }
            }
        }

        private async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            if (!_requestBuilder.IsBatch)
            {
                using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                    IEnumerable<IDictionary<string, object>> result = null;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Created:
                            var responseReader = _session.Provider.GetResponseReader();
                            var odataResponse = await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties);
                            return odataResponse.Entries ?? new[] { odataResponse.Entry };
                            break;

                        default:
                            result = Enumerable.Empty<IDictionary<string, object>>();
                            break;
                    }
                    return result;
                }
            }
            else
            {
                return await Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
            }
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

        private Task<string> FormatEntryKeyAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            return GetFluentClient()
                .For(_session.MetadataCache.FindBaseEntitySet(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
        }
    }
}
