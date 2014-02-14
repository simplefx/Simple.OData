using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public static ISchema GetSchema(string urlBase, ICredentials credentials = null)
        {
            return Client.Schema.Get(urlBase, credentials);
        }

        public static string GetSchemaAsString(string urlBase, ICredentials credentials = null)
        {
            return SchemaProvider.FromUrl(urlBase, credentials).SchemaAsString;
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText)
        {
            int totalCount;
            return FindEntries(commandText, false, false, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult)
        {
            int totalCount;
            return FindEntries(commandText, scalarResult, false, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount)
        {
            return FindEntries(commandText, false, true, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, out int totalCount)
        {
            return FindEntries(commandText, scalarResult, true, out totalCount);
        }

        public IDictionary<string, object> FindEntry(string commandText)
        {
            int totalCount;
            var result = FindEntries(commandText, false, false, out totalCount);
            return result == null ? null : result.FirstOrDefault();
        }

        public object FindScalar(string commandText)
        {
            int totalCount;
            var result = FindEntries(commandText, true, false, out totalCount);
            return result == null ? null : result.FirstOrDefault().Values.First();
        }

        public IDictionary<string, object> GetEntry(string collection, params object[] entryKey)
        {
            var entryKeyWithNames = new Dictionary<string, object>();
            var keyNames = _schema.FindConcreteTable(collection).GetKeyNames();
            for (int index = 0; index < keyNames.Count; index++)
            {
                entryKeyWithNames.Add(keyNames[index], entryKey.ElementAt(index));
            }
            return GetEntry(collection, entryKeyWithNames);
        }

        public IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey)
        {
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateGetCommand(commandText);
            var request = _requestBuilder.CreateRequest(command);
            return _requestRunner.GetEntry(request);
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
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
            var request = _requestBuilder.CreateRequest(command);
            var result = _requestRunner.InsertEntry(request, resultRequired);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = commandWriter.CreateLinkCommand(collection, associatedData.Key, command.ContentId, associatedData.Value);
                request = _requestBuilder.CreateRequest(linkCommand);
                _requestRunner.InsertEntry(request, resultRequired);
            }

            return result;
        }

        public int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData)
        {
            RemoveSystemProperties(entryData);
            return IterateEntries(collection, commandText, entryData, UpdateEntry);
        }

        public int UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(entryData);
            var table = _schema.FindConcreteTable(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            return UpdateEntryPropertiesAndAssociations(collection, entryKey, entryData, entryMembers);
        }

        public int DeleteEntries(string collection, string commandText)
        {
            return IterateEntries(collection, commandText, null, (x, y, z) => DeleteEntry(x, y));
        }

        public int DeleteEntry(string collection, IDictionary<string, object> entryKey)
        {
            RemoveSystemProperties(entryKey);
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateDeleteCommand(commandText);
            var request = _requestBuilder.CreateRequest(command);
            return _requestRunner.DeleteEntry(request);
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
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
            var request = _requestBuilder.CreateRequest(command);
            _requestRunner.UpdateEntry(request);
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            RemoveSystemProperties(entryKey);
            var association = _schema.FindAssociation(collection, linkName);
            var commandText = GetFluentClient()
                .For(collection)
                .Key(entryKey)
                .CommandText;

            var command = new CommandWriter(_schema).CreateUnlinkCommand(collection, association.ActualName, commandText);
            var request = _requestBuilder.CreateRequest(command);
            _requestRunner.UpdateEntry(request);
        }

        public IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var commandText = GetFluentClient()
                .Function(functionName)
                .Parameters(parameters)
                .CommandText;

            var command = new HttpCommand(function.HttpMethod.ToUpper(), commandText);
            var request = _requestBuilder.CreateRequest(command);
            return _requestRunner.ExecuteFunction(request);
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return (T)ExecuteFunction(functionName, parameters).First().First().Value;
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunction(functionName, parameters)
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
        }
    }
}
