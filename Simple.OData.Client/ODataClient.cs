using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Simple.OData.Client
{
    internal class EntryMembers
    {
        private IDictionary<string, object> _properties = new Dictionary<string, object>();
        private List<KeyValuePair<string, object>> _associationsByValue = new List<KeyValuePair<string, object>>();
        private List<KeyValuePair<string, int>> _associationsByContentId = new List<KeyValuePair<string, int>>();

        public IDictionary<string, object> Properties { get { return _properties; } }
        public List<KeyValuePair<string, object>> AssociationsByValue { get { return _associationsByValue; } }
        public List<KeyValuePair<string, int>> AssociationsByContentId { get { return _associationsByContentId; } }

        public void AddProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        public void AddAssociationByValue(string associationName, object associatedData)
        {
            _associationsByValue.Add(new KeyValuePair<string, object>(associationName, associatedData));
        }

        public void AddAssociationByContentId(string associationName, int contentId)
        {
            _associationsByContentId.Add(new KeyValuePair<string, int>(associationName, contentId));
        }
    }

    public class ODataClient
    {
        private string _urlBase;
        private ISchema _schema;
        private RequestBuilder _requestBuilder;
        private RequestRunner _requestRunner;

        public ODataClient(string urlBase)
        {
            _urlBase = urlBase;
            _schema = Client.Schema.Get(urlBase);

            _requestBuilder = new CommandRequestBuilder(_urlBase);
            _requestRunner = new CommandRequestRunner();
        }

        public ODataClient(ODataBatch batch)
        {
            _urlBase = batch.RequestBuilder.UrlBase;
            _schema = Client.Schema.Get(_urlBase);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string SchemaAsString
        {
            get { return SchemaProvider.FromUrl(_urlBase).SchemaAsString; }
        }

        public static ISchema GetSchema(string urlBase)
        {
            return Client.Schema.Get(urlBase);
        }

        public static string GetSchemaAsString(string urlBase)
        {
            return SchemaProvider.FromUrl(urlBase).SchemaAsString;
        }

        public static ISchema ParseSchemaString(string schemaString)
        {
            return SchemaProvider.FromMetadata(schemaString).Schema;
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
        }

        public IDictionary<string, object> FindEntry(string commandText)
        {
            int totalCount;
            var result = FindEntries(commandText, false, false, out totalCount);
            return result == null ? null : result.FirstOrDefault();
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

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool setTotalCount, out int totalCount)
        {
            return FindEntries(commandText, false, setTotalCount, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.FindEntries(command, scalarResult, setTotalCount, out totalCount);
        }

        public IDictionary<string, object> GetEntry(string tableName, IDictionary<string, object> entryKey)
        {
            var commandText = FormatGetKeyCommand(tableName, entryKey);
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.GetEntry(command);
        }

        public IDictionary<string, object> InsertEntry(string tableName, IDictionary<string, object> entryData, bool resultRequired)
        {
            var entryMembers = ParseEntryMembers(tableName, entryData);

            var entry = ODataHelper.CreateDataElement(entryMembers.Properties);
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                CreateLinkElement(entry, tableName, associatedData);
            }

            var commandText = GetTableActualName(tableName);
            var command = HttpCommand.Post(commandText, entryData, entry.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.InsertEntry(command, resultRequired);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, associatedData.Key, 
                    ODataHelper.CreateLinkPath(command.ContentId), 
                    ODataHelper.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.InsertEntry(linkCommand, resultRequired);
            }

            return result;
        }

        public int UpdateEntry(string tableName, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var entryMembers = ParseEntryMembers(tableName, entryData);
            if (entryMembers.Properties.Count == 0)
            {
                return UpdateEntryAssociations(tableName, entryKey, entryData, entryMembers);
            }
            else
            {
                return UpdateEntryPropertiesAndAssociations(tableName, entryKey, entryData, entryMembers);
            }
        }

        public int DeleteEntry(string tableName, IDictionary<string, object> entryKey)
        {
            var commandText = FormatGetKeyCommand(tableName, entryKey);
            var command = HttpCommand.Delete(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.DeleteEntry(command);
        }

        public void LinkEntry(string tableName, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            var association = _schema.FindTable(tableName).FindAssociation(linkName);
            var command = CreateLinkCommand(tableName, linkName, 
                FormatGetKeyCommand(tableName, entryKey), 
                FormatGetKeyCommand(association.ReferenceTableName, linkedEntryKey));
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public void UnlinkEntry(string tableName, IDictionary<string, object> entryKey, string linkName)
        {
            var association = _schema.FindTable(tableName).FindAssociation(linkName);
            var command = CreateUnlinkCommand(tableName, linkName, FormatGetKeyCommand(tableName, entryKey));
            _requestBuilder.AddCommandToRequest(command);
            _requestRunner.UpdateEntry(command);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            var function = _schema.FindFunction(functionName);
            var formattedParameters = new ValueFormatter().Format(parameters, "&");
            var commandText = function.ActualName + "?" + formattedParameters;
            var command = new HttpCommand(function.HttpMethod.ToUpper(), commandText.ToString());
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.ExecuteFunction(command);
        }

        private int UpdateEntryAssociations(string tableName, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, EntryMembers entryMembers)
        {
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                var association = _schema.FindTable(tableName).FindAssociation(associatedData.Key);
                if (associatedData.Value != null)
                {
                    var associatedKeyValues = GetLinkedEntryKeyValues(association.ReferenceTableName, associatedData);
                    if (associatedKeyValues != null)
                    {
                        LinkEntry(tableName, entryKey, association.ActualName, GetLinkedEntryProperties(associatedData.Value));
                    }
                }
                else
                {
                    UnlinkEntry(tableName, entryKey, association.ActualName);
                }
            }
            return 0;
        }

        private int UpdateEntryPropertiesAndAssociations(string tableName, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, EntryMembers entryMembers)
        {
            bool merge = CheckMergeConditions(tableName, entryKey, entryData);
            var commandText = FormatGetKeyCommand(tableName, entryKey);

            var entryElement = ODataHelper.CreateDataElement(entryMembers.Properties);
            foreach (var associatedData in entryMembers.AssociationsByValue)
            {
                CreateLinkElement(entryElement, tableName, associatedData);
            }

            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, entryData, entryElement.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntry(command);

            foreach (var associatedData in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, associatedData.Key, 
                    ODataHelper.CreateLinkPath(command.ContentId), 
                    ODataHelper.CreateLinkPath(associatedData.Value));
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntry(linkCommand);
            }

            return result;
        }

        private HttpCommand CreateLinkCommand(string tableName, string associationName, string entryPath, string linkPath)
        {
            var linkEntry = ODataHelper.CreateLinkElement(linkPath);
            var linkMethod = _schema.FindTable(tableName).FindAssociation(associationName).IsMultiple ? 
                RestVerbs.POST : 
                RestVerbs.PUT;

            var commandText = ODataHelper.CreateLinkCommand(entryPath, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private HttpCommand CreateUnlinkCommand(string tableName, string associationName, string entryPath)
        {
            var commandText = ODataHelper.CreateLinkCommand(entryPath, associationName);
            return HttpCommand.Delete(commandText);
        }

        private void CreateLinkElement(XElement entry, string tableName, KeyValuePair<string, object> associatedData)
        {
            if (associatedData.Value == null)
                return;

            var association = _schema.FindTable(tableName).FindAssociation(associatedData.Key);
            var associatedKeyValues = GetLinkedEntryKeyValues(association.ReferenceTableName, associatedData);
            if (associatedKeyValues != null)
            {
                ODataHelper.AddDataLink(entry, association.ActualName, association.ReferenceTableName, associatedKeyValues);
            }
        }

        private IEnumerable<object> GetLinkedEntryKeyValues(string tableName, KeyValuePair<string, object> entryData)
        {
            var entryProperties = GetLinkedEntryProperties(entryData.Value);
            var associatedKeyNames = _schema.FindTable(tableName).GetKeyNames();
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

        private EntryMembers ParseEntryMembers(string tableName, IDictionary<string, object> entryData)
        {
            var entryMembers = new EntryMembers();

            var table = _schema.FindTable(tableName);
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

        private string GetTableActualName(string tableName)
        {
            return _schema.FindTable(tableName).ActualName;
        }

        private string FormatGetKeyCommand(string tableName, IDictionary<string, object> entryKey)
        {
            var keyNames = _schema.FindTable(tableName).GetKeyNames();
            var keyValues = new List<object>();
            foreach (var keyName in keyNames)
            {
                object keyValue;
                if (entryKey.TryGetValue(keyName, out keyValue))
                {
                    keyValues.Add(keyValue);
                }
            }
            var formattedKeyValues = new ValueFormatter().Format(keyValues);
            return GetTableActualName(tableName) + "(" + formattedKeyValues + ")";
        }

        private bool CheckMergeConditions(string tableName, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            var keyNames = _schema.FindTable(tableName).GetKeyNames();
            foreach (var key in entryKey.Keys)
            {
                if (!keyNames.Contains(key) && !entryData.Keys.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
