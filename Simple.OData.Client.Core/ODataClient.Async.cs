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
        public static async Task<object> GetMetadataAsync(string urlBase)
        {
            return (await Session.FromUrl(urlBase, null).ResolveAsync(CancellationToken.None)).Provider.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<object> GetMetadataAsync(string urlBase, CancellationToken cancellationToken)
        {
            return (await Session.FromUrl(urlBase, null).ResolveAsync(cancellationToken)).Provider.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<object> GetMetadataAsync(string urlBase, ICredentials credentials)
        {
            return (await Session.FromUrl(urlBase, credentials).ResolveAsync(CancellationToken.None)).Provider.Model;
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
            return (await Session.FromUrl(urlBase, credentials).ResolveAsync(cancellationToken)).Provider.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<T> GetMetadataAsync<T>(string urlBase)
        {
            return (T)(await Session.FromUrl(urlBase, null).ResolveAsync(CancellationToken.None)).Provider.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<T> GetMetadataAsync<T>(string urlBase, CancellationToken cancellationToken)
        {
            return (T)(await Session.FromUrl(urlBase, null).ResolveAsync(cancellationToken)).Provider.Model;
        }

        /// <summary>
        /// Retrieves the OData service metadata.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="urlBase">The URL base of the OData service.</param>
        /// <param name="credentials">The OData service access credentials.</param>
        /// <returns>The service metadata.</returns>
        public static async Task<T> GetMetadataAsync<T>(string urlBase, ICredentials credentials)
        {
            return (T)(await Session.FromUrl(urlBase, credentials).ResolveAsync(CancellationToken.None)).Provider.Model;
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
            return (T)(await Session.FromUrl(urlBase, credentials).ResolveAsync(cancellationToken)).Provider.Model;
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
            return await new ProviderFactory(urlBase, credentials).GetMetadataAsStringAsync(cancellationToken);
        }

        #pragma warning disable 1591

        internal async Task<Session> GetSessionAsync()
        {
            return (await _session.ResolveAsync(CancellationToken.None));
        }

        public async Task<object> GetMetadataAsync()
        {
            return (await _session.ResolveAsync(CancellationToken.None)).Provider.Model;
        }

        public async Task<object> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return (await _session.ResolveAsync(cancellationToken)).Provider.Model;
        }

        public async Task<T> GetMetadataAsync<T>()
        {
            return (T)(await _session.ResolveAsync(CancellationToken.None)).Provider.Model;
        }

        public async Task<T> GetMetadataAsync<T>(CancellationToken cancellationToken)
        {
            return (T)(await _session.ResolveAsync(cancellationToken)).Provider.Model;
        }

        public Task<string> GetMetadataAsStringAsync()
        {
            return GetMetadataAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetMetadataAsStringAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            return _session.MetadataAsString;
        }

        public Task<string> GetCommandTextAsync(string collection, ODataExpression expression)
        {
            return GetCommandTextAsync(collection, expression, CancellationToken.None);
        }

        public async Task<string> GetCommandTextAsync(string collection, ODataExpression expression, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
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
            await _session.ResolveAsync(cancellationToken);
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

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await RetrieveEntriesAsync(commandText, false, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult)
        {
            return FindEntriesAsync(commandText, scalarResult, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await RetrieveEntriesAsync(commandText, scalarResult, cancellationToken);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText)
        {
            return FindEntriesWithCountAsync(commandText, CancellationToken.None);
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await RetrieveEntriesWithCountAsync(commandText, false, cancellationToken);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult)
        {
            return FindEntriesWithCountAsync(commandText, scalarResult, CancellationToken.None);
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await RetrieveEntriesWithCountAsync(commandText, scalarResult, cancellationToken);
        }

        public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            return FindEntryAsync(commandText, CancellationToken.None);
        }

        public async Task<IDictionary<string, object>> FindEntryAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var result = await RetrieveEntriesAsync(commandText, false, cancellationToken);
            return result == null ? null : result.FirstOrDefault();
        }

        public Task<object> FindScalarAsync(string commandText)
        {
            return FindScalarAsync(commandText, CancellationToken.None);
        }

        public async Task<object> FindScalarAsync(string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var result = await RetrieveEntriesAsync(commandText, true, cancellationToken);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            return GetEntryAsync(collection, CancellationToken.None, entryKey);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, CancellationToken cancellationToken, params object[] entryKey)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _session.FindConcreteEntitySet(collection).GetKeyNames();
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
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = new CommandWriter(_session).CreateGetCommand(commandText);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.GetEntryAsync(request, cancellationToken);
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
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryData);
            var entitySet = _session.FindConcreteEntitySet(collection);
            var entryMembers = ParseEntryMembers(entitySet, entryData);

            var commandWriter = new CommandWriter(_session);
            var entryContent = commandWriter.CreateEntry(
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            var command = commandWriter.CreateInsertCommand(_session.FindBaseEntitySet(collection).ActualName, entryData, entryContent);
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
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);
            var table = _session.FindConcreteEntitySet(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = await GetFluentClient()
                .For(_session.FindBaseEntitySet(collection).ActualName)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);

            var commandWriter = new CommandWriter(_session);
            var entryContent = commandWriter.CreateEntry(
                _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection),
                _session.Provider.GetMetadata().GetEntitySetTypeName(collection),
                entryMembers.Properties,
                entryMembers.AssociationsByValue,
                entryMembers.AssociationsByContentId);

            var command = commandWriter.CreateUpdateCommand(commandText, entryData, entryContent, merge);
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
                .Select(x => _session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, x.Key))
                .ToList();

            foreach (var associationName in unlinkAssociationNames)
            {
                await UnlinkEntryAsync(collection, entryKey, associationName, cancellationToken);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
            //return await UpdateEntryPropertiesAndAssociationsAsync(collection, entryKey, entryData, entryMembers, resultRequired, cancellationToken);
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
            await _session.ResolveAsync(cancellationToken);
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
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = new CommandWriter(_session).CreateDeleteCommand(commandText);
            var request = _requestBuilder.CreateRequest(command, false, _session.Provider.GetMetadata()
                .EntitySetTypeRequiresOptimisticConcurrencyCheck(collection));
            await _requestRunner.DeleteEntryAsync(request, cancellationToken);
        }

        public Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            return DeleteEntriesAsync(collection, commandText, CancellationToken.None);
        }

        public async Task<int> DeleteEntriesAsync(string collection, string commandText, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
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
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);

            var entryPath = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var linkPath = await GetFluentClient()
                .For(_session.Provider.GetMetadata().GetNavigationPropertyPartnerName(collection, linkName))
                .Key(linkedEntryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = new CommandWriter(_session).CreateLinkCommand(
                collection, _session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName), entryPath, linkPath);
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.UpdateEntryAsync(request, cancellationToken);
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            return UnlinkEntryAsync(collection, entryKey, linkName, CancellationToken.None);
        }

        public async Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            RemoveSystemProperties(entryKey);
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = new CommandWriter(_session).CreateUnlinkCommand(
                collection, _session.Provider.GetMetadata().GetNavigationPropertyExactName(collection, linkName), commandText);
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.UpdateEntryAsync(request, cancellationToken);
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsync(functionName, parameters, CancellationToken.None);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await GetFluentClient()
                .Function(functionName)
                .Parameters(parameters)
                .GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var command = new HttpCommand(RestVerbs.GET, commandText);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.ExecuteFunctionAsync(request, cancellationToken);
        }

        public Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsScalarAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (T)(await ExecuteFunctionAsync(functionName, parameters, cancellationToken)).First().First().Value;
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsArrayAsync<T>(functionName, parameters, CancellationToken.None);
        }

        public async Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return (await ExecuteFunctionAsync(functionName, parameters, cancellationToken))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        #pragma warning restore 1591

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, cancellationToken);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntriesWithCountAsync(commandText, scalarResult, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> FindEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindEntryAsync(commandText, cancellationToken);
        }

        internal async Task<object> FindScalarAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await FindScalarAsync(commandText, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> InsertEntryAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            return await InsertEntryAsync(collectionName, entryData, resultRequired, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            return await UpdateEntryAsync(collectionName, entryKey, entryData, resultRequired, cancellationToken);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            return await UpdateEntryAsync(collectionName, command.KeyValues, command.EntryData, resultRequired, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await UpdateEntriesAsync(collectionName, commandText, entryData, resultRequired, cancellationToken);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            await DeleteEntryAsync(collectionName, entryKey, cancellationToken);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            await DeleteEntryAsync(collectionName, command.KeyValues, cancellationToken);
        }

        internal async Task<int> DeleteEntriesAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await DeleteEntriesAsync(collectionName, commandText, cancellationToken);
        }

        internal async Task LinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            await LinkEntryAsync(collectionName, entryKey, linkName, linkedEntryKey, cancellationToken);
        }

        internal async Task UnlinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _session.FindEntitySet(command.CollectionName).ActualName;
            await UnlinkEntryAsync(collectionName, entryKey, linkName, cancellationToken);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsync(commandText, parameters, cancellationToken);
        }

        internal async Task<T> ExecuteFunctionAsScalarAsync<T>(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsScalarAsync<T>(commandText, parameters, cancellationToken);
        }

        internal async Task<T[]> ExecuteFunctionAsArrayAsync<T>(FluentCommand command, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            await _session.ResolveAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await command.GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await ExecuteFunctionAsArrayAsync<T>(commandText, parameters, cancellationToken);
        }
    }
}
