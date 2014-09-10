using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
        public static async Task<object> GetMetadataAsync(string urlBase, ICredentials credentials, CancellationToken cancellationToken)
        {
            var session = Session.FromUrl(urlBase, credentials);
            await session.ResolveProviderAsync(cancellationToken);
            return session.Provider.Model;
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
            var session = Session.FromUrl(urlBase, credentials);
            await session.ResolveProviderAsync(cancellationToken);
            return (T)session.Provider.Model;
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
            var session = Session.FromUrl(urlBase, credentials);
            await session.ResolveProviderAsync(cancellationToken);
            return session.MetadataCache.MetadataAsString;
        }

        #pragma warning disable 1591

        internal async Task<Session> GetSessionAsync()
        {
            await _session.ResolveProviderAsync(CancellationToken.None);
            return _session;
        }

        public async Task<object> GetMetadataAsync()
        {
            return (await _session.ResolveProviderAsync(CancellationToken.None)).Model;
        }

        public async Task<object> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return (await _session.ResolveProviderAsync(cancellationToken)).Model;
        }

        public async Task<T> GetMetadataAsync<T>()
        {
            return (T)(await _session.ResolveProviderAsync(CancellationToken.None)).Model;
        }

        public async Task<T> GetMetadataAsync<T>(CancellationToken cancellationToken)
        {
            return (T)(await _session.ResolveProviderAsync(cancellationToken)).Model;
        }

        public Task<string> GetMetadataAsStringAsync()
        {
            return GetMetadataAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetMetadataAsStringAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            return _session.MetadataCache.MetadataAsString;
        }

        public Task<string> GetCommandTextAsync(string collection, ODataExpression expression)
        {
            return GetCommandTextAsync(collection, expression, CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(string collection, ODataExpression expression, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await GetFluentClient()
                .For(collection)
                .Filter(expression.Format(_session, collection))
                .GetCommandTextAsync(cancellationToken);
        }

        public Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression)
        {
            return GetCommandTextAsync(collection, expression, CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await GetFluentClient()
                .For(collection)
                .Filter(ODataExpression.FromLinqExpression(expression.Body).Format(_session, collection))
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateGetRequestAsync(commandText, scalarResult);
            return await ExecuteFindEntriesRequestAsync(request, cancellationToken);
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateGetRequestAsync(commandText, scalarResult);
            return await ExecuteFindEntriesWithCountRequestAsync(request, cancellationToken);
        }

        public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            return FindEntryAsync(commandText, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> FindEntryAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateGetRequestAsync(commandText);
            var result = await ExecuteFindEntriesRequestAsync(request, cancellationToken);
            return result == null ? null : result.FirstOrDefault();
        }

        public Task<object> FindScalarAsync(string commandText)
        {
            return FindScalarAsync(commandText, CancellationToken.None);
        }

        public async Task<object> FindScalarAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateGetRequestAsync(commandText, true);
            var result = await ExecuteFindEntriesRequestAsync(request, cancellationToken);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            return GetEntryAsync(collection, CancellationToken.None, entryKey);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, CancellationToken cancellationToken, params object[] entryKey)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _session.MetadataCache.FindConcreteEntitySet(collection).GetKeyNames();
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await FormatEntryKeyAsync(collection, entryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateGetRequestAsync(commandText);
            return await ExecuteGetEntryRequestAsync(request, cancellationToken);
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await InsertEntryAndLinksAsync(collection, entryData, resultRequired, cancellationToken);
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await UpdateEntryAndLinksAsync(collection, entryKey, entryData, resultRequired, cancellationToken);
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
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryData);
            return await IterateEntriesAsync(
                collection, commandText, entryData, resultRequired,
                async (x, y, z, w) => await UpdateEntryAsync(x, y, z, w, cancellationToken), 
                cancellationToken);
        }

        public Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return DeleteEntryAsync(collection, entryKey, CancellationToken.None);
        }

        public async Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateDeleteRequestAsync(commandText, collection);
            await ExecuteDeleteEntryRequestAsync(request, cancellationToken);
        }

        public Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            return DeleteEntriesAsync(collection, commandText, CancellationToken.None);
        }

        public async Task<int> DeleteEntriesAsync(string collection, string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await IterateEntriesAsync(
                collection, commandText,
                async (x, y) => await DeleteEntryAsync(x, y, cancellationToken), 
                cancellationToken);
        }

        public Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return LinkEntryAsync(collection, entryKey, linkName, linkedEntryKey, CancellationToken.None);
        }

        public async Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);

            var entryPath = await FormatEntryKeyAsync(collection, entryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var linkedCollection = _session.Provider.GetMetadata().GetNavigationPropertyPartnerName(collection, linkName);
            var linkPath = await FormatEntryKeyAsync(linkedCollection, linkedEntryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateLinkRequestAsync(collection, linkName, entryPath, linkPath);

            if (!_requestBuilder.IsBatch)
            {
                using (await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                }
            }
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            return UnlinkEntryAsync(collection, entryKey, linkName, CancellationToken.None);
        }

        public async Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            var commandText = await FormatEntryKeyAsync(collection, entryKey, cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _requestBuilder.CreateUnlinkRequestAsync(commandText, collection, linkName);

            if (!_requestBuilder.IsBatch)
            {
                using (await _requestRunner.ExecuteRequestAsync(request, cancellationToken))
                {
                }
            }
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsync(functionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await GetFluentClient()
                .Function(functionName)
                .Parameters(parameters)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = new ODataRequest(RestVerbs.GET, this.Session, commandText);
            return await ExecuteFunctionRequestAsync(request, cancellationToken);
        }

        public Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsScalarAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (T)(await ExecuteFunctionAsync(functionName, parameters, cancellationToken)).First().First().Value;
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsArrayAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteFunctionAsync(functionName, parameters, cancellationToken))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        #pragma warning restore 1591

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> FindEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntryAsync(commandText, cancellationToken);
        }

        internal async Task<object> FindScalarAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindScalarAsync(commandText, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> InsertEntryAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            return await InsertEntryAsync(collectionName, entryData, resultRequired, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            return await UpdateEntryAsync(collectionName, entryKey, entryData, resultRequired, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            return await UpdateEntryAsync(collectionName, command.KeyValues, command.EntryData, resultRequired, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await UpdateEntriesAsync(collectionName, commandText, entryData, resultRequired, cancellationToken);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            await DeleteEntryAsync(collectionName, entryKey, cancellationToken);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            await DeleteEntryAsync(collectionName, command.KeyValues, cancellationToken);
        }

        internal async Task<int> DeleteEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await DeleteEntriesAsync(collectionName, commandText, cancellationToken);
        }

        internal async Task LinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            await LinkEntryAsync(collectionName, entryKey, linkName, linkedEntryKey, cancellationToken);
        }

        internal async Task UnlinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.MetadataCache.FindEntitySet(command.CollectionName).ActualName;
            await UnlinkEntryAsync(collectionName, entryKey, linkName, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsync(commandText, parameters, cancellationToken);
        }

        internal async Task<T> ExecuteFunctionAsScalarAsync<T>(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsScalarAsync<T>(commandText, parameters, cancellationToken);
        }

        internal async Task<T[]> ExecuteFunctionAsArrayAsync<T>(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveProviderAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsArrayAsync<T>(commandText, parameters, cancellationToken);
        }
    }
}
