using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public static Task<ISchema> GetSchemaAsync(string urlBase, ICredentials credentials = null)
        {
            return Task.Factory.StartNew(() => Client.Schema.FromUrl(urlBase, credentials));
        }

        public static async Task<string> GetSchemaAsStringAsync(string urlBase, ICredentials credentials = null)
        {
            var requestBuilder = new CommandRequestBuilder(urlBase, credentials);
            var command = HttpCommand.Get(FluentCommand.MetadataLiteral);
            requestBuilder.AddCommandToRequest(command);
            var requestRunner = new SchemaRequestRunner(new ODataClientSettings());
            using (var response = await requestRunner.ExecuteRequestAsync(command.Request))
            {
                return ResponseReader.GetSchemaAsString(response.GetResponseStream());
            }
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText)
        {
            return RetrieveEntriesAsync(commandText, false);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult)
        {
            return RetrieveEntriesAsync(commandText, scalarResult);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText)
        {
            return RetrieveEntriesWithCountAsync(commandText, false);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(string commandText, bool scalarResult)
        {
            return RetrieveEntriesWithCountAsync(commandText, scalarResult);
        }

        public async Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            var result = await RetrieveEntriesAsync(commandText, false);
            return result == null ? null : result.FirstOrDefault();
        }

        public async Task<object> FindScalarAsync(string commandText)
        {
            var result = await RetrieveEntriesAsync(commandText, true);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _schema.FindConcreteTable(collection).GetKeyNames();
            for (int index = 0; index < keyNames.Count; index++)
            {
                entryKeyWithNames.Add(keyNames[index], entryKey.ElementAt(index));
            }
            return GetEntryAsync(collection, entryKeyWithNames);
        }

        public async Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateGetCommand(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return await _requestRunner.GetEntryAsync(command);
        }

        public async Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
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
            _requestBuilder.AddCommandToRequest(command);
            var result = await _requestRunner.InsertEntryAsync(command, resultRequired);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                _requestBuilder.AddCommandToRequest(linkCommand);
                await _requestRunner.InsertEntryAsync(linkCommand, resultRequired);
            }

            return result;
        }

        public Task<int> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData)
        {
            RemoveSystemProperties(entryData);
            return IterateEntriesAsync(collection, commandText, entryData, UpdateEntry);
        }

        public Task<int> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);
            var table = _schema.FindConcreteTable(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            return UpdateEntryPropertiesAndAssociationsAsync(collection, entryKey, entryData, entryMembers);
        }

        public Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            return IterateEntriesAsync(collection, commandText, null, (x, y, z) => DeleteEntry(x, y));
        }

        public Task<int> DeleteEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            RemoveSystemProperties(entryKey);
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateDeleteCommand(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.DeleteEntryAsync(command);
        }

        public Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);
            var association = _schema.FindAssociation(collection, linkName);

            var entryPath = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;
            var linkPath = GetFluentClient()
                .For(association.ReferenceTableName)
                .Key(linkedEntryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateLinkCommand(collection, association.ActualName, entryPath, linkPath);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.UpdateEntryAsync(command);
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            RemoveSystemProperties(entryKey);
            var association = _schema.FindAssociation(collection, linkName);
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateUnlinkCommand(collection, association.ActualName, commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.UpdateEntryAsync(command);
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var commandText = GetFluentClient()
                .Function(functionName)
                .Parameters(parameters)
                .CommandText;

            var command = new HttpCommand(function.HttpMethod.ToUpper(), commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.ExecuteFunctionAsync(command);
        }

        public async Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return (T)(await ExecuteFunctionAsync(functionName, parameters)).First().First().Value;
        }

        public async Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return (await ExecuteFunctionAsync(functionName, parameters))
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }
    }
}
