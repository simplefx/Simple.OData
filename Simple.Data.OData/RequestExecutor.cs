using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
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

    internal class RequestExecutor
    {
        private string _urlBase;
        private DatabaseSchema _schema;
        private RequestBuilder _requestBuilder;
        private RequestRunner _requestRunner;

        public RequestExecutor(string urlBase, DatabaseSchema schema, IAdapterTransaction transaction = null)
        {
            _urlBase = urlBase;
            _schema = schema;

            _requestBuilder = transaction == null
                                     ? new CommandRequestBuilder(_urlBase)
                                     : (transaction as ODataAdapterTransaction).RequestBuilder;
            _requestRunner = transaction == null
                                    ? new CommandRequestRunner()
                                    : (transaction as ODataAdapterTransaction).RequestRunner;
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var command = HttpCommand.Get(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.FindEntries(command, scalarResult, setTotalCount, out totalCount);
        }

        public IDictionary<string, object> InsertEntry(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            var entryMembers = ParseEntryMembers(tableName, data);

            var entry = DataServicesHelper.CreateDataElement(entryMembers.Properties);
            foreach (var association in entryMembers.AssociationsByValue)
            {
                CreateLink(entry, tableName, association);
            }

            var commandText = GetTableActualName(tableName);
            var command = HttpCommand.Post(commandText, data, entry.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.InsertEntry(command, resultRequired);

            foreach (var association in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, association.Key, command.ContentId, association.Value);
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.InsertEntry(linkCommand, false);
            }

            return result;
        }

        public int UpdateEntry(string tableName, string keys, IDictionary<string, object> updatedData, bool merge, IAdapterTransaction transaction)
        {
            Dictionary<string, object> allData = new Dictionary<string, object>();
            updatedData.Keys.ToList().ForEach(x => allData.Add(x, updatedData[x]));

            var entryMembers = ParseEntryMembers(tableName, allData);

            var entry = DataServicesHelper.CreateDataElement(entryMembers.Properties);
            foreach (var association in entryMembers.AssociationsByValue)
            {
                CreateLink(entry, tableName, association);
            }

            var commandText = GetTableActualName(tableName) + "(" + keys + ")";
            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, updatedData, entry.ToString());
            _requestBuilder.AddCommandToRequest(command);
            var result = _requestRunner.UpdateEntry(command);

            foreach (var association in entryMembers.AssociationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, association.Key, command.ContentId, association.Value);
                _requestBuilder.AddCommandToRequest(linkCommand);
                _requestRunner.UpdateEntry(linkCommand);
            }

            return result;
        }

        public int DeleteEntry(string tableName, string keys, IAdapterTransaction transaction)
        {
            var commandText = GetTableActualName(tableName) + "(" + keys + ")";
            var command = HttpCommand.Delete(commandText);
            _requestBuilder.AddCommandToRequest(command);
            return _requestRunner.DeleteEntry(command);
        }

        private HttpCommand CreateLinkCommand(string tableName, string associationName, int entryContentId, int linkContentId)
        {
            var linkEntry = DataServicesHelper.CreateLinkElement(linkContentId);
            var linkMethod = _schema.FindTable(tableName).FindAssociation(associationName).IsMultiple ? RestVerbs.POST : RestVerbs.PUT;

            var commandText = string.Format("${0}/$links/{1}", entryContentId, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private void CreateLink(XElement entry, string tableName, KeyValuePair<string, object> associatedData)
        {
            if (associatedData.Value == null)
                return;

            var association = _schema.FindTable(tableName).FindAssociation(associatedData.Key);
            var entryProperties = GetLinkedEntryProperties(associatedData.Value);
            var keyFieldNames = _schema.FindTable(association.ReferenceTableName).PrimaryKey.AsEnumerable().ToArray();
            var keyFieldValues = new object[keyFieldNames.Count()];

            for (int index = 0; index < keyFieldNames.Count(); index++)
            {
                bool ok = entryProperties.TryGetValue(keyFieldNames[index], out keyFieldValues[index]);
                if (!ok)
                    return;
            }
            DataServicesHelper.AddDataLink(entry, association.ActualName, association.ReferenceTableName, keyFieldValues);
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

        private EntryMembers ParseEntryMembers(string tableName, IDictionary<string, object> data)
        {
            var entryMembers = new EntryMembers();

            var table = _schema.FindTable(tableName);
            foreach (var item in data)
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
                throw new SimpleDataException(string.Format("No property or association found for {0}.", item.Key));
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
    }
}