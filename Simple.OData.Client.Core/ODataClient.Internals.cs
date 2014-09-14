using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private async Task<T> ExecuteRequestWithResultAsync<T>(ODataRequest request, CancellationToken cancellationToken,
            Func<ODataResponse, T> createResult, Func<T> createEmptyResult = null, Func<T> createBatchResult = null)
        {
            if (_requestBuilder.IsBatch)
                return createBatchResult != null ? createBatchResult() : default (T);

            try
            {
                using (var response = await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                    if (response.IsSuccessStatusCode && (request.Method == RestVerbs.Get || request.ReturnContent))
                    {
                        var responseReader = _session.Provider.GetResponseReader();
                        return createResult(await responseReader.GetResponseAsync(response, _settings.IncludeResourceTypeInEntryProperties));
                    }
                    else
                    {
                        return default (T);
                    }
                }
            }
            catch (WebRequestException ex)
            {
                if (_settings.IgnoreResourceNotFoundException && ex.Code == HttpStatusCode.NotFound)
                    return createEmptyResult != null ? createEmptyResult() : default (T);
                else
                    throw;
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
                result = new[] { await funcAsync(collection, entryKey, entryData, resultRequired) };
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
                .For(_session.Metadata.GetBaseEntityCollection(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
        }
    }
}
