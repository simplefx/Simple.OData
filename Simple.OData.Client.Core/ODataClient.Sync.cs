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
            var commandText = new ODataClientWithCommand(this, _schema).For(collection).Key(entryKey).CommandText;
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.GetEntry(command);
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            RemoveSystemProperties(entryData);
            var table = _schema.FindConcreteTable(collection);
            var entryMembers = ParseEntryMembers(table, entryData);

            var feedWriter = new ODataFeedWriter();
            var entry = feedWriter.CreateDataElement(_schema.TypesNamespace, table.ActualName, entryMembers.Properties);
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                CreateLinkElement(entry, collection, associatedData);
            }

            var commandText = _schema.FindBaseTable(collection).ActualName;
            var command = HttpCommand.Post(commandText, entryData, entry.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.InsertEntry(command, resultRequired);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(collection, associatedData.Key,
                    feedWriter.CreateLinkPath(command.ContentId),
                    feedWriter.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.InsertEntry(linkCommand, resultRequired);
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
            var commandText = new ODataClientWithCommand(this, _schema).For(collection).Key(entryKey).CommandText;
            var command = HttpCommand.Delete(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.DeleteEntry(command);
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            RemoveSystemProperties(entryKey);
            RemoveSystemProperties(linkedEntryKey);
            var association = _schema.FindAssociation(collection, linkName);
            var command = CreateLinkCommand(collection, linkName,
                new ODataClientWithCommand(this, _schema).For(collection).Key(entryKey).CommandText,
                new ODataClientWithCommand(this, _schema).For(association.ReferenceTableName).Key(linkedEntryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            RemoveSystemProperties(entryKey);
            var command = CreateUnlinkCommand(collection, linkName, 
                new ODataClientWithCommand(this, _schema).For(collection).Key(entryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var command = new HttpCommand(function.HttpMethod.ToUpper(), 
                new ODataClientWithCommand(this, _schema).Function(functionName).Parameters(parameters).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.ExecuteFunction(command);
        }
    }
}
