using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations.
    /// </summary>
    public partial class ODataClient
    {
        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <returns>The service metadata.</returns>
        public static Task<object> GetMetadataAsync(string urlBase)
        {
            return GetMetadataAsync(urlBase, null, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static Task<object> GetMetadataAsync(string urlBase, CancellationToken cancellationToken)
        {
            return GetMetadataAsync(urlBase, null, cancellationToken);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <returns>The service metadata.</returns>
        public static Task<object> GetMetadataAsync(string urlBase, ICredentials credentials)
        {
            return GetMetadataAsync(urlBase, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static Task<object> GetMetadataAsync(string urlBase, ICredentials credentials, CancellationToken cancellationToken)
        {
            return GetMetadataAsync<object>(urlBase, credentials, cancellationToken);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <returns>The service metadata.</returns>
        public static Task<T> GetMetadataAsync<T>(string urlBase)
        {
            return GetMetadataAsync<T>(urlBase, null, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static Task<T> GetMetadataAsync<T>(string urlBase, CancellationToken cancellationToken)
        {
            return GetMetadataAsync<T>(urlBase, null, cancellationToken);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <returns>The service metadata.</returns>
        public static Task<T> GetMetadataAsync<T>(string urlBase, ICredentials credentials)
        {
            return GetMetadataAsync<T>(urlBase, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The service metadata.
        /// </returns>
        public static async Task<T> GetMetadataAsync<T>(string urlBase, ICredentials credentials, CancellationToken cancellationToken)
        {
            var session = Session.FromSettings(new ODataClientSettings(urlBase, credentials));
            await session.ResolveAdapterAsync(cancellationToken);
            return (T)session.Adapter.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata as string.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <returns>The service metadata.</returns>
        public static Task<string> GetMetadataAsStringAsync(string urlBase)
        {
            return GetMetadataAsStringAsync(urlBase, null, CancellationToken.None);
        }

        /// <summary>
        /// Gets The service metadata as string asynchronous.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static Task<string> GetMetadataAsStringAsync(string urlBase, CancellationToken cancellationToken)
        {
            return GetMetadataAsStringAsync(urlBase, null, CancellationToken.None);
        }

        /// <summary>
        /// Gets The service metadata as string asynchronous.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <returns>The service metadata.</returns>
        public static Task<string> GetMetadataAsStringAsync(string urlBase, ICredentials credentials)
        {
            return GetMetadataAsStringAsync(urlBase, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Gets The service metadata as string asynchronous.
        /// </summary>
        /// <param name="urlBase">The URL base.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<string> GetMetadataAsStringAsync(string urlBase, ICredentials credentials, CancellationToken cancellationToken)
        {
            var session = Session.FromSettings(new ODataClientSettings(urlBase, credentials));
            await session.ResolveAdapterAsync(cancellationToken);
            return session.MetadataCache.MetadataAsString;
        }

        #pragma warning disable 1591

        internal async Task<Session> GetSessionAsync()
        {
            await _session.ResolveAdapterAsync(CancellationToken.None);
            return _session;
        }

        public async Task<object> GetMetadataAsync()
        {
            return (await _session.ResolveAdapterAsync(CancellationToken.None)).Model;
        }

        public async Task<object> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return (await _session.ResolveAdapterAsync(cancellationToken)).Model;
        }

        public async Task<T> GetMetadataAsync<T>()
        {
            return (T)(await _session.ResolveAdapterAsync(CancellationToken.None)).Model;
        }

        public async Task<T> GetMetadataAsync<T>(CancellationToken cancellationToken)
        {
            return (T)(await _session.ResolveAdapterAsync(cancellationToken)).Model;
        }

        public Task<string> GetMetadataAsStringAsync()
        {
            return GetMetadataAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetMetadataAsStringAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            return _session.MetadataCache.MetadataAsString;
        }

        public Task<string> GetCommandTextAsync(string collection, ODataExpression expression)
        {
            return GetCommandTextAsync(collection, expression, CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(string collection, ODataExpression expression, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await GetFluentClient()
                .For(collection)
                .Filter(expression)
                .GetCommandTextAsync(cancellationToken);
        }

        public Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression)
        {
            return GetCommandTextAsync(collection, expression, CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await GetFluentClient()
                .For(collection)
                .Filter(ODataExpression.FromLinqExpression(expression.Body))
                .GetCommandTextAsync(cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText)
        {
            return FindEntriesAsync(commandText, CancellationToken.None);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, CancellationToken cancellationToken)
        {
            return FindEntriesAsync(commandText, false, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult)
        {
            return FindEntriesAsync(commandText, scalarResult, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entries;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(commandText, scalarResult);

            return await ExecuteRequestWithResultAsync(request, cancellationToken,
                x => x.Entries ?? new[] { x.Entry },
                () => new[] { (IDictionary<string, object>)null });
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText)
        {
            return FindEntriesWithCountAsync(commandText, CancellationToken.None);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, CancellationToken cancellationToken)
        {
            return FindEntriesWithCountAsync(commandText, false, CancellationToken.None);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult)
        {
            return FindEntriesWithCountAsync(commandText, scalarResult, CancellationToken.None);
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return new Tuple<IEnumerable<IDictionary<string, object>>, int>(
                    _batchResponse.Entries, 
                    _batchResponse.TotalCount.HasValue ? (int)_batchResponse.TotalCount.Value : 0);

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(commandText, scalarResult);

            return await ExecuteRequestWithResultAsync(request, cancellationToken,
                x => Tuple.Create(x.Entries, (int)x.TotalCount.GetValueOrDefault()),
                () => new Tuple<IEnumerable<IDictionary<string, object>>, int>(new[] { (IDictionary<string, object>)null }, 0));
        }

        public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            return FindEntryAsync(commandText, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> FindEntryAsync(string commandText, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(commandText, false);

            var result = await ExecuteRequestWithResultAsync(request, cancellationToken,
                x => x.Entries ?? new[] { x.Entry },
                () => new[] { (IDictionary<string, object>)null });
            return result == null ? null : result.FirstOrDefault();
        }

        public Task<object> FindScalarAsync(string commandText)
        {
            return FindScalarAsync(commandText, CancellationToken.None);
        }

        public async Task<object> FindScalarAsync(string commandText, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry == null 
                    ? null 
                    : (object)_batchResponse.Entry.First();

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(commandText, true);

            var result = await ExecuteRequestWithResultAsync(request, cancellationToken,
                x => x.Entries ?? new[] { x.Entry },
                () => new[] { (IDictionary<string, object>)null });
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            return GetEntryAsync(collection, CancellationToken.None, entryKey);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, CancellationToken cancellationToken, params object[] entryKey)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entryKeyWithNames = new Dictionary<string, object>();
            var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
            var keyNames = _session.Metadata.GetDeclaredKeyPropertyNames(entityCollection.ActualName).ToList();
            for (int index = 0; index < keyNames.Count; index++)
            {
                entryKeyWithNames.Add(keyNames[index], entryKey.ElementAt(index));
            }
            return await GetEntryAsync(collection, entryKeyWithNames, cancellationToken);
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return GetEntryAsync(collection, entryKey, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entryIdent = await FormatEntryKeyAsync(collection, entryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateGetRequestAsync(entryIdent, false);

            return await ExecuteRequestWithResultAsync(request, cancellationToken, x => x.Entry, () => null);
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData)
        {
            return InsertEntryAsync(collection, entryData, true, CancellationToken.None);
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, CancellationToken cancellationToken)
        {
            return InsertEntryAsync(collection, entryData, true, cancellationToken);
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired)
        {
            return InsertEntryAsync(collection, entryData, resultRequired, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryData);

            var command = GetFluentClient()
                .For(collection)
                .Set(entryData)
                .AsFluentClient().Command;

            return await ExecuteInsertEntryAsync(command, resultRequired, cancellationToken);
        }

        public Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return UpdateEntryAsync(collection, entryKey, entryData, true, CancellationToken.None);
        }

        public Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, CancellationToken cancellationToken)
        {
            return UpdateEntryAsync(collection, entryKey, entryData, true, cancellationToken);
        }

        public Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired)
        {
            return UpdateEntryAsync(collection, entryKey, entryData, resultRequired, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entry;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);

            var command = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .Set(entryData)
                .AsFluentClient().Command;

            return await ExecuteUpdateEntryAsync(command, resultRequired, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData)
        {
            return UpdateEntriesAsync(collection, commandText, entryData, true, CancellationToken.None);
        }

        public Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, CancellationToken cancellationToken)
        {
            return UpdateEntriesAsync(collection, commandText, entryData, true, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired)
        {
            return UpdateEntriesAsync(collection, commandText, entryData, resultRequired, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entries;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryData);

            var command = GetFluentClient()
                .For(collection)
                .Filter(ExtractFilterFromCommandText(collection, commandText))
                .Set(entryData)
                .AsFluentClient().Command;

            return await ExecuteUpdateEntriesAsync(command, resultRequired, cancellationToken);
        }

        public Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return DeleteEntryAsync(collection, entryKey, CancellationToken.None);
        }

        public async Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);

            var command = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .AsFluentClient().Command;

            await ExecuteDeleteEntryAsync(command, cancellationToken);
        }

        public Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            return DeleteEntriesAsync(collection, commandText, CancellationToken.None);
        }

        public async Task<int> DeleteEntriesAsync(string collection, string commandText, CancellationToken cancellationToken)
        {
            // TODO
            if (_batchResponse != null)
                return 0;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .For(collection)
                .Filter(ExtractFilterFromCommandText(collection, commandText))
                .AsFluentClient().Command;

            return await ExecuteDeleteEntriesAsync(command, cancellationToken);
        }

        public Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return LinkEntryAsync(collection, entryKey, linkName, linkedEntryKey, CancellationToken.None);
        }

        public async Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);

            var command = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .AsFluentClient().Command;

            await ExecuteLinkEntryAsync(command, linkName, linkedEntryKey, cancellationToken);
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            return UnlinkEntryAsync(collection, entryKey, linkName, null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken)
        {
            return UnlinkEntryAsync(collection, entryKey, linkName, null, cancellationToken);
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return UnlinkEntryAsync(collection, entryKey, linkName, linkedEntryKey, CancellationToken.None);
        }

        public async Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);

            var command = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .AsFluentClient().Command;

            await ExecuteUnlinkEntryAsync(command, linkName, linkedEntryKey, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsync(functionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entries;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Function(functionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return await ExecuteFunctionAsync(command, cancellationToken);
        }

        public Task<IEnumerable<T>> ExecuteFunctionAsEntriesAsync<T>(string functionName, IDictionary<string, object> parameters)
            where T : class
        {
            return ExecuteFunctionAsEntriesAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<T>> ExecuteFunctionAsEntriesAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
            where T : class
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Function(functionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return (await ExecuteAsEntriesAsync<T>(command, cancellationToken));
        }

        public Task<T> ExecuteFunctionAsEntryAsync<T>(string functionName, IDictionary<string, object> parameters)
            where T : class
        {
            return ExecuteFunctionAsEntryAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteFunctionAsEntryAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
            where T : class
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Function(functionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return (await ExecuteAsEntryAsync<T>(command, cancellationToken));
        }

        public Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsScalarAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            // TODO
            if (_batchResponse != null)
                return default(T);

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (T)(await ExecuteFunctionAsync(functionName, parameters, cancellationToken)).First().First().Value;
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsArrayAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteFunctionAsync(functionName, parameters, cancellationToken))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteActionAsync(string actionName, IDictionary<string, object> parameters)
        {
            return ExecuteActionAsync(actionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteActionAsync(string actionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            if (_batchResponse != null)
                return _batchResponse.Entries;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Action(actionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return await ExecuteActionAsync(command, cancellationToken);
        }

        public Task<IEnumerable<T>> ExecuteActionAsEntriesAsync<T>(string actionName, IDictionary<string, object> parameters)
            where T : class
        {
            return ExecuteActionAsEntriesAsync<T>(actionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<T>> ExecuteActionAsEntriesAsync<T>(string actionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
            where T : class
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Action(actionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return (await ExecuteAsEntriesAsync<T>(command, cancellationToken));
        }

        public Task<T> ExecuteActionAsEntryAsync<T>(string actionName, IDictionary<string, object> parameters)
            where T : class
        {
            return ExecuteActionAsEntryAsync<T>(actionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteActionAsEntryAsync<T>(string actionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
            where T : class
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = GetFluentClient()
                .Action(actionName)
                .Set(parameters)
                .AsFluentClient().Command;

            return (await ExecuteAsEntryAsync<T>(command, cancellationToken));
        }

        public Task<T> ExecuteActionAsScalarAsync<T>(string actionName, IDictionary<string, object> parameters)
        {
            return ExecuteActionAsScalarAsync<T>(actionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteActionAsScalarAsync<T>(string actionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            // TODO
            if (_batchResponse != null)
                return default(T);

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (T)(await ExecuteActionAsync(actionName, parameters, cancellationToken)).First().First().Value;
        }

        public Task<T[]> ExecuteActionAsArrayAsync<T>(string actionName, IDictionary<string, object> parameters)
        {
            return ExecuteActionAsArrayAsync<T>(actionName, parameters, CancellationToken.None);
        }

        public async Task<T[]> ExecuteActionAsArrayAsync<T>(string actionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            // TODO
            if (_batchResponse != null)
                return null;

            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteActionAsync(actionName, parameters, cancellationToken))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        #pragma warning restore 1591

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> FindEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntryAsync(commandText, cancellationToken);
        }

        internal async Task<object> FindScalarAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindScalarAsync(commandText, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> InsertEntryAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteInsertEntryAsync(new FluentCommand(command).Set(entryData), resultRequired, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteUpdateEntryAsync(command, resultRequired, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteUpdateEntriesAsync(command, resultRequired, cancellationToken);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            await ExecuteDeleteEntryAsync(command, cancellationToken);
        }

        internal async Task<int> DeleteEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteDeleteEntriesAsync(command, cancellationToken);
        }

        internal async Task LinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            await ExecuteLinkEntryAsync(new FluentCommand(command).Key(entryKey), linkName, linkedEntryKey, cancellationToken);
        }

        internal async Task UnlinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            await ExecuteUnlinkEntryAsync(new FluentCommand(command).Key(entryKey), linkName, linkedEntryKey, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> ExecuteAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            if (command.HasFunction)
                return await ExecuteFunctionAsync(command, cancellationToken);
            else if (command.HasAction)
                return await ExecuteActionAsync(command, cancellationToken);
            else
                throw new InvalidOperationException("Command is expected to be a function or an action.");
        }

        internal async Task<IEnumerable<T>> ExecuteAsEntriesAsync<T>(FluentCommand command, CancellationToken cancellationToken) 
            where T : class
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<IDictionary<string, object>> result;
            if (command.HasFunction)
                result = await ExecuteFunctionAsync(command, cancellationToken);
            else if (command.HasAction)
                result = await ExecuteActionAsync(command, cancellationToken);
            else
                throw new InvalidOperationException("Command is expected to be a function or an action.");

            return result.Select(x => x.ToObject<T>());
        }

        internal async Task<T> ExecuteAsEntryAsync<T>(FluentCommand command, CancellationToken cancellationToken)
            where T : class
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteAsEntriesAsync<T>(command, cancellationToken)).FirstOrDefault();
        }

        internal async Task<T> ExecuteAsScalarAsync<T>(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (T)(await ExecuteAsync(command, cancellationToken)).First().First().Value;
        }

        internal async Task<T[]> ExecuteAsArrayAsync<T>(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteAsync(command, cancellationToken))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        internal async Task ExecuteBatchAsync(IList<Func<IODataClient, Task>> actions, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            await ExecuteBatchActionsAsync(actions, cancellationToken);
        }

        private string ExtractFilterFromCommandText(string collection, string commandText)
        {
            const string filterPrefix = "?$filter=";

            if (commandText.Length > filterPrefix.Length &&
                commandText.Substring(0, collection.Length + filterPrefix.Length).Equals(
                    collection + filterPrefix, StringComparison.CurrentCultureIgnoreCase))
            {
                return commandText.Substring(collection.Length + filterPrefix.Length);
            }
            else
            {
                return commandText;
            }
        }
    }
}
