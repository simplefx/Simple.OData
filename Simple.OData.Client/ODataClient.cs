using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace Simple.OData.Client
{
    public class ODataClient
    {
        private readonly ODataClientSettings _settings;
        private readonly ISchema _schema;
        private readonly RequestBuilder _requestBuilder;
        private readonly RequestRunner _requestRunner;

        public ODataClient(string urlBase)
            : this(new ODataClientSettings { UrlBase = urlBase })
        {
        }

        public ODataClient(ODataClientSettings settings)
        {
            _settings = settings;
            _schema = Client.Schema.Get(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = new CommandRequestBuilder(_settings.UrlBase, _settings.Credentials);
            _requestRunner = new CommandRequestRunner(_settings.IncludeResourceTypeInEntryProperties);
            _requestRunner.BeforeRequest = _settings.BeforeRequest;
            _requestRunner.AfterResponse = _settings.AfterResponse;
        }

        public ODataClient(ODataBatch batch)
        {
            _settings = batch.Settings;
            _schema = Client.Schema.Get(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string SchemaAsString
        {
            get { return SchemaProvider.FromUrl(_settings.UrlBase, _settings.Credentials).SchemaAsString; }
        }

        public static ISchema GetSchema(string urlBase, ICredentials credentials = null)
        {
            return Client.Schema.Get(urlBase, credentials);
        }

        public static string GetSchemaAsString(string urlBase, ICredentials credentials = null)
        {
            return SchemaProvider.FromUrl(urlBase, credentials).SchemaAsString;
        }

        public static ISchema ParseSchemaString(string schemaString)
        {
            return SchemaProvider.FromMetadata(schemaString).Schema;
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
        }

        public IClientWithCommand From(string collectionName)
        {
            return new ODataClientWithCommand(this, _schema).From(collectionName);
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

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.FindEntries(command, scalarResult, setTotalCount, out totalCount);
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
            var commandText = new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText;
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.GetEntry(command);
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired)
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
            var commandText = new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText;
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
                new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText,
                new ODataClientWithCommand(this, _schema).From(association.ReferenceTableName).Key(linkedEntryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            RemoveSystemProperties(entryKey);
            var association = _schema.FindAssociation(collection, linkName);
            var command = CreateUnlinkCommand(collection, linkName, new ODataClientWithCommand(this, _schema).From(collection).Key(entryKey).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var command = new HttpCommand(function.HttpMethod.ToUpper(), new ODataClientWithCommand(this, _schema).Function(functionName).Parameters(parameters).CommandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.ExecuteFunction(command);
        }

        public string FormatFilter(string collection, dynamic filterExpression)
        {
            if (filterExpression is FilterExpression)
            {
                var clientWithCommand = new ODataClientWithCommand(this, _schema);
                string filter = (filterExpression as FilterExpression)
                    .Format(clientWithCommand, collection);

                return clientWithCommand
                    .From(collection)
                    .Filter(filter).CommandText;
            }
            else
            {
                throw new InvalidOperationException("Unable to cast dynamic object to FilterExpression");
            }
        }

        private int IterateEntries(string collection, string commandText, IDictionary<string, object> entryData,
            Func<string, IDictionary<string, object>, IDictionary<string, object>, int> func)
        {
            var entryKey = ExtractKeyFromCommandText(collection, commandText);
            if (entryKey != null)
            {
                return func(collection, entryKey, entryData);
            }
            else
            {
                var entries = new ODataClient(_settings).FindEntries(commandText);
                if (entries != null)
                {
                    var entryList = entries.ToList();
                    foreach (var entry in entryList)
                    {
                        func(collection, entry, entryData);
                    }
                    return entryList.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int UpdateEntryPropertiesAndAssociations(
            string collection,
            IDictionary<string, object> entryKey,
            IDictionary<string, object> entryData,
            EntryMembers entryMembers)
        {
            bool hasPropertiesToUpdate = entryMembers.Properties.Count > 0;
            bool merge = !hasPropertiesToUpdate || CheckMergeConditions(collection, entryKey, entryData);
            var commandText = new ODataClientWithCommand(this, _schema).From(_schema.FindBaseTable(collection).ActualName).Key(entryKey).CommandText;

            var feedWriter = new ODataFeedWriter();
            var table = _schema.FindConcreteTable(collection);
            var entryElement = feedWriter.CreateDataElement(_schema.TypesNamespace, table.ActualName, entryMembers.Properties);
            var unlinkAssociationNames = new List<string>();
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var association = table.FindAssociation(associatedData.Key);
                if (associatedData.Value != null)
                {
                    CreateLinkElement(entryElement, collection, associatedData);
                }
                else
                {
                    unlinkAssociationNames.Add(association.ActualName);
                }
            }

            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, entryData, entryElement.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntry(command);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(collection, associatedData.Key,
                    feedWriter.CreateLinkPath(command.ContentId),
                    feedWriter.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntry(linkCommand);
            }

            foreach (var associationName in unlinkAssociationNames)
            {
                UnlinkEntry(collection, entryKey, associationName);
            }

            return result;
        }

        private HttpCommand CreateLinkCommand(string collection, string associationName, string entryPath, string linkPath)
        {
            var feedWriter = new ODataFeedWriter();
            var linkEntry = feedWriter.CreateLinkElement(linkPath);
            var linkMethod = _schema.FindAssociation(collection, associationName).IsMultiple ?
                RestVerbs.POST :
                RestVerbs.PUT;

            var commandText = feedWriter.CreateLinkCommand(entryPath, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private HttpCommand CreateUnlinkCommand(string collection, string associationName, string entryPath)
        {
            var commandText = new ODataFeedWriter().CreateLinkCommand(entryPath, associationName);
            return HttpCommand.Delete(commandText);
        }

        private void CreateLinkElement(XElement entry, string collection, KeyValuePair<string, object> associatedData)
        {
            if (associatedData.Value == null)
                return;

            var association = _schema.FindAssociation(collection, associatedData.Key);
            var associatedKeyValues = GetLinkedEntryKeyValues(association.ReferenceTableName, associatedData);
            if (associatedKeyValues != null)
            {
                new ODataFeedWriter().AddDataLink(entry, association.ActualName, association.ReferenceTableName, associatedKeyValues);
            }
        }

        private IEnumerable<object> GetLinkedEntryKeyValues(string collection, KeyValuePair<string, object> entryData)
        {
            var entryProperties = GetLinkedEntryProperties(entryData.Value);
            var associatedKeyNames = _schema.FindConcreteTable(collection).GetKeyNames();
            var associatedKeyValues = new object[associatedKeyNames.Count()];
            for (int index = 0; index < associatedKeyNames.Count(); index++)
            {
                bool ok = entryProperties.TryGetValue(associatedKeyNames[index], out associatedKeyValues[index]);
                if (!ok)
                    return null;
            }
            return associatedKeyValues;
        }

        private IDictionary<string, object> GetLinkedEntryProperties(object entryData)
        {
            IDictionary<string, object> entryProperties = entryData as IDictionary<string, object>;
            if (entryProperties == null)
            {
                entryProperties = new Dictionary<string, object>();
                var entryType = entryData.GetType();
                foreach (var entryProperty in entryType.GetProperties())
                {
                    entryProperties.Add(entryProperty.Name, entryType.GetProperty(entryProperty.Name).GetValue(entryData, null));
                }
            }
            return entryProperties;
        }

        private EntryMembers ParseEntryMembers(Table table, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            foreach (var item in entryData)
            {
                ParseEntryMember(table, item, entryMembers);
            }

            return entryMembers;
        }

        private void ParseEntryMember(Table table, KeyValuePair<string, object> item, EntryMembers entryMembers)
        {
            if (table.HasColumn(item.Key))
            {
                entryMembers.AddProperty(item.Key, item.Value);
            }
            else if (table.HasAssociation(item.Key))
            {
                var association = table.FindAssociation(item.Key);
                if (association.IsMultiple)
                {
                    var collection = item.Value as IEnumerable<object>;
                    if (collection != null)
                    {
                        foreach (var element in collection)
                        {
                            AddEntryAssociation(entryMembers, item.Key, element);
                        }
                    }
                }
                else
                {
                    AddEntryAssociation(entryMembers, item.Key, item.Value);
                }
            }
            else
            {
                throw new UnresolvableObjectException(item.Key, string.Format("No property or association found for {0}.", item.Key));
            }
        }

        private void AddEntryAssociation(EntryMembers entryMembers, string associationName, object associatedData)
        {
            int contentId = _requestBuilder.GetContentId(associatedData);
            if (contentId == 0)
            {
                entryMembers.AddAssociationByValue(associationName, associatedData);
            }
            else
            {
                entryMembers.AddAssociationByContentId(associationName, contentId);
            }
        }

        private bool CheckMergeConditions(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var table = _schema.FindConcreteTable(collection);
            var keyNames = table.GetKeyNames();
            foreach (var key in entryKey.Keys)
            {
                if (!keyNames.Contains(key) && !entryData.Keys.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveSystemProperties(IDictionary<string, object> entryData)
        {
            if (_settings.IncludeResourceTypeInEntryProperties && entryData.ContainsKey(ODataCommand.ResourceTypeLiteral))
            {
                entryData.Remove(ODataCommand.ResourceTypeLiteral);
            }
        }

        private IDictionary<string, object> ExtractKeyFromCommandText(string collection, string commandText)
        {
            // TODO
            return null;
        }
    }
}
