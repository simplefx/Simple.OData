using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public static Task<ISchema> GetSchemaAsync(string urlBase, ICredentials credentials = null)
        {
            return Schema.FromUrl(urlBase, credentials).ResolveAsync();
        }

        public static async Task<string> GetSchemaAsStringAsync(string urlBase, ICredentials credentials = null)
        {
            var requestBuilder = new CommandRequestBuilder(urlBase, credentials);
            var command = HttpCommand.Get(FluentCommand.MetadataLiteral);
            var request = requestBuilder.CreateRequest(command);
            var requestRunner = new SchemaRequestRunner(new ODataClientSettings());
            using (var response = await requestRunner.ExecuteRequestAsync(request))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public Task<ISchema> GetSchemaAsync()
        {
            return _schema.ResolveAsync();
        }

        public async Task<string> GetSchemaAsStringAsync()
        {
            await _schema.ResolveAsync();
            return _schema.MetadataAsString;
        }

        public async Task<string> GetCommandTextAsync(string collection, ODataExpression expression)
        {
            await _schema.ResolveAsync();
            return await GetFluentClient()
                .For(collection)
                .Filter(expression.Format(_schema, collection))
                .GetCommandTextAsync();
        }

        public async Task<string> GetCommandTextAsync<T>(string collection, Expression<Func<T, bool>> expression)
        {
            await _schema.ResolveAsync();
            return await GetFluentClient()
                .For(collection)
                .Filter(ODataExpression.FromLinqExpression(expression.Body).Format(_schema, collection))
                .GetCommandTextAsync();
        }

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText)
        {
            await _schema.ResolveAsync();
            return await RetrieveEntriesAsync(commandText, false);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult)
        {
            await _schema.ResolveAsync();
            return await RetrieveEntriesAsync(commandText, scalarResult);
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText)
        {
            await _schema.ResolveAsync();
            return await RetrieveEntriesWithCountAsync(commandText, false);
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult)
        {
            await _schema.ResolveAsync();
            return await RetrieveEntriesWithCountAsync(commandText, scalarResult);
        }

        public async Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            await _schema.ResolveAsync();
            var result = await RetrieveEntriesAsync(commandText, false);
            return result == null ? null : result.FirstOrDefault();
        }

        public async Task<object> FindScalarAsync(string commandText)
        {
            await _schema.ResolveAsync();
            var result = await RetrieveEntriesAsync(commandText, true);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            await _schema.ResolveAsync();
            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _schema.FindConcreteTable(collection).GetKeyNames();
            for (int index = 0; index < keyNames.Count; index++)
            {
                entryKeyWithNames.Add(keyNames[index], entryKey.ElementAt(index));
            }
            return await GetEntryAsync(collection, entryKeyWithNames);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            await _schema.ResolveAsync();
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync();

            var command = new CommandWriter(_schema).CreateGetCommand(commandText);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.GetEntryAsync(request);
        }

        public async Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryData);
            var table = _schema.FindConcreteTable(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            var commandWriter = new CommandWriter(_schema);
            var entryContent = commandWriter.CreateEntry(table.EntityType.Name, entryMembers.Properties);
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                commandWriter.AddLink(entryContent, collection, associatedData);
            }

            var command = commandWriter.CreateInsertCommand(_schema.FindBaseTable(collection).ActualName, entryData, entryContent);
            var request = _requestBuilder.CreateRequest(command, resultRequired);
            var result = await _requestRunner.InsertEntryAsync(request);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand, resultRequired);
                await _requestRunner.InsertEntryAsync(request);
            }

            return result;
        }

        public async Task<IDictionary<string, object>> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);
            var table = _schema.FindConcreteTable(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            return await UpdateEntryPropertiesAndAssociationsAsync(collection, entryKey, entryData, entryMembers, resultRequired);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryData);
            return await IterateEntriesAsync(
                collection, commandText, entryData, resultRequired, 
                async (x, y, z, w) => await UpdateEntryAsync(x, y, z, w));
        }

        public async Task DeleteEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryKey);
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync();

            var command = new CommandWriter(_schema).CreateDeleteCommand(commandText);
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.DeleteEntryAsync(request);
        }

        public async Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            await _schema.ResolveAsync();
            return await IterateEntriesAsync(
                collection, commandText, 
                async (x, y) => await DeleteEntryAsync(x, y));
        }

        public async Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);
            var association = _schema.FindAssociation(collection, linkName);

            var entryPath = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync();
            var linkPath = await GetFluentClient()
                .For(association.ReferenceTableName)
                .Key(linkedEntryKey)
                .GetCommandTextAsync();

            var command = new CommandWriter(_schema).CreateLinkCommand(collection, association.ActualName, entryPath, linkPath);
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.UpdateEntryAsync(request);
        }

        public async Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            await _schema.ResolveAsync();
            RemoveSystemProperties(entryKey);
            var association = _schema.FindAssociation(collection, linkName);
            var commandText = await GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync();

            var command = new CommandWriter(_schema).CreateUnlinkCommand(collection, association.ActualName, commandText);
            var request = _requestBuilder.CreateRequest(command);
            await _requestRunner.UpdateEntryAsync(request);
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();
            var function = _schema.FindFunction(functionName);
            var commandText = await GetFluentClient()
                .Function(functionName)
                .Parameters(parameters)
                .GetCommandTextAsync();

            var command = new HttpCommand(function.HttpMethod.ToUpper(), commandText);
            var request = _requestBuilder.CreateRequest(command);
            return await _requestRunner.ExecuteFunctionAsync(request);
        }

        public async Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();
            return (T)(await ExecuteFunctionAsync(functionName, parameters)).First().First().Value;
        }

        public async Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();

            return (await ExecuteFunctionAsync(functionName, parameters))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindEntriesAsync(commandText);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(FluentCommand command, bool scalarResult)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindEntriesAsync(commandText, scalarResult);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindEntriesWithCountAsync(commandText);
        }

        internal async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(FluentCommand command, bool scalarResult)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindEntriesWithCountAsync(commandText, scalarResult);
        }

        internal async Task<IDictionary<string, object>> FindEntryAsync(FluentCommand command)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindEntryAsync(commandText);
        }

        internal async Task<object> FindScalarAsync(FluentCommand command)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await FindScalarAsync(commandText);
        }

        internal async Task<IDictionary<string, object>> InsertEntryAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            return await InsertEntryAsync(collectionName, entryData, resultRequired);
        }

        internal async Task<IDictionary<string, object>> UpdateEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            return await UpdateEntryAsync(collectionName, entryKey, entryData, resultRequired);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> UpdateEntriesAsync(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync();
            return await UpdateEntriesAsync(collectionName, commandText, entryData, resultRequired);
        }

        internal async Task DeleteEntryAsync(FluentCommand command, IDictionary<string, object> entryKey)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            await DeleteEntryAsync(collectionName, entryKey);
        }

        internal async Task<int> DeleteEntriesAsync(FluentCommand command)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            var commandText = await command.GetCommandTextAsync();
            return await DeleteEntriesAsync(collectionName, commandText);
        }

        internal async Task LinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            await LinkEntryAsync(collectionName, entryKey, linkName, linkedEntryKey);
        }

        internal async Task UnlinkEntryAsync(FluentCommand command, IDictionary<string, object> entryKey, string linkName)
        {
            await _schema.ResolveAsync();
            var collectionName = _schema.FindTable(command.CollectionName).ActualName;
            await UnlinkEntryAsync(collectionName, entryKey, linkName);
        }

        internal async Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(FluentCommand command, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await ExecuteFunctionAsync(commandText, parameters);
        }

        internal async Task<T> ExecuteFunctionAsScalarAsync<T>(FluentCommand command, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await ExecuteFunctionAsScalarAsync<T>(commandText, parameters);
        }

        internal async Task<T[]> ExecuteFunctionAsArrayAsync<T>(FluentCommand command, IDictionary<string, object> parameters)
        {
            await _schema.ResolveAsync();
            var commandText = await command.GetCommandTextAsync();
            return await ExecuteFunctionAsArrayAsync<T>(commandText, parameters);
        }
    }
}
