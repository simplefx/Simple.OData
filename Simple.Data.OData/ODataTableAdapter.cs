using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
{
    using System.ComponentModel.Composition;
    using Simple.Data.OData;

    [Export("OData", typeof(Adapter))]
    public partial class ODataTableAdapter : Adapter
    {
        private string _urlBase;
        private ExpressionFormatter _expressionFormatter;
        private DatabaseSchema _schema;

        internal string UrlBase
        {
            get { return _urlBase; }
        }

        internal DatabaseSchema GetSchema()
        {
            return _schema ?? (_schema = DatabaseSchema.Get(_urlBase));
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            _urlBase = Settings.Url;
            _expressionFormatter = new ExpressionFormatter(DatabaseSchema.Get(_urlBase).FindTable);
            _schema = DatabaseSchema.Get(_urlBase);
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return FindByExpression(tableName, criteria);
        }

        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            return new Table(tableName, GetSchema()).GetKey(tableName, record);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            return new Table(tableName, GetSchema()).GetKeyNames();
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            return FindByKey(tableName, keyValues);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return FindByQuery(query, out unhandledClauses);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            CheckInsertablePropertiesAreAvailable(tableName, data);
            return InsertEntry(tableName, data, null, resultRequired);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return UpdateByExpression(tableName, data, criteria, null);
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            return DeleteByExpression(tableName, criteria, null);
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return false;
        }

        private IEnumerable<IDictionary<string, object>> FindByExpression(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter(GetSchema().FindTable).Format(criteria);
            var builder = new CommandBuilder(GetSchema().FindTable);
            var commandText = builder.BuildCommand(GetTableActualName(tableName), filter);

            return FindEntries(commandText);
        }

        private IEnumerable<IDictionary<string, object>> FindByQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = new CommandBuilder(GetSchema().FindTable);
            string command = builder.BuildCommand(query);
            unhandledClauses = builder.UnprocessedClauses;
            IEnumerable<IDictionary<string, object>> results;

            if (builder.SetTotalCount == null)
            {
                results = FindEntries(command, builder.IsScalarResult);
            }
            else
            {
                int totalCount;
                results = FindEntries(command, out totalCount);
                builder.SetTotalCount(totalCount);
            }
            return results;
        }

        private IDictionary<string, object> FindByKey(string tableName, object[] keyValues)
        {
            var keyNames = GetKeyNames(tableName);
            var namedKeyValues = new Dictionary<string, object>();
            for (int index = 0; index < keyValues.Count(); index++)
            {
                namedKeyValues.Add(keyNames[index], keyValues[index]);
            }
            var formattedKeyValues = new ExpressionFormatter(GetSchema().FindTable).Format(namedKeyValues);
            return FindByKey(tableName, formattedKeyValues).FirstOrDefault();
        }

        private IEnumerable<IDictionary<string, object>> FindByKey(string tableName, string keys)
        {
            return FindEntries(GetTableActualName(tableName) + "(" + keys + ")");
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult = false)
        {
            int totalCount;
            return FindEntries(commandText, scalarResult, false, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount)
        {
            return FindEntries(commandText, false, true, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var requestBuilder = new CommandRequestBuilder(_urlBase);
            var command = HttpCommand.Get(commandText);
            requestBuilder.AddCommandToRequest(command);
            return new CommandRequestRunner().FindEntries(command, scalarResult, setTotalCount, out totalCount);
        }

        private IDictionary<string, object> InsertEntry(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            RequestBuilder requestBuilder;
            RequestRunner requestRunner;
            GetRequestHandlers(transaction, out requestBuilder, out requestRunner);

            IDictionary<string, object> properties;
            IDictionary<string, object> associationsByValue;
            IDictionary<string, int> associationsByContentId;
            VerifyEntryData(requestBuilder, tableName, data, out properties, out associationsByValue, out associationsByContentId);

            var entry = DataServicesHelper.CreateDataElement(properties);
            foreach (var association in associationsByValue)
            {
                CreateLink(entry, tableName, association);
            }

            var commandText = GetTableActualName(tableName);
            var command = HttpCommand.Post(commandText, data, entry.ToString());
            requestBuilder.AddCommandToRequest(command);
            var result = requestRunner.InsertEntry(command, resultRequired);

            foreach (var association in associationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, association.Key, command.ContentId, association.Value);
                requestBuilder.AddCommandToRequest(linkCommand);
                requestRunner.InsertEntry(linkCommand, false);
            }

            return result;
        }

        private int UpdateByExpression(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            // TODO: optimize
            string[] keyFieldNames = GetSchema().FindTable(tableName).PrimaryKey.AsEnumerable().ToArray();
            var entries = FindByExpression(tableName, criteria);

            foreach (var entry in entries)
            {
                var namedKeyValues = new Dictionary<string, object>();
                for (int index = 0; index < keyFieldNames.Count(); index++)
                {
                    namedKeyValues.Add(keyFieldNames[index], entry[keyFieldNames[index]]);
                }
                var formattedKeyValues = _expressionFormatter.Format(namedKeyValues);
                var unaffectedData = new Dictionary<string, object>();
                bool merge = false;
                foreach (var item in entry)
                {
                    if (!keyFieldNames.Contains(item.Key) && !data.ContainsKey(item.Key))
                    {
                        merge = true;
                        break;
                    }
                }
                UpdateEntry(tableName, formattedKeyValues, data, merge, transaction);
            }
            // TODO: what to return?
            return 0;
        }

        private int UpdateEntry(string tableName, string keys, IDictionary<string, object> updatedData, bool merge, IAdapterTransaction transaction)
        {
            Dictionary<string, object> allData = new Dictionary<string, object>();
            updatedData.Keys.ToList().ForEach(x => allData.Add(x, updatedData[x]));

            RequestBuilder requestBuilder;
            RequestRunner requestRunner;
            GetRequestHandlers(transaction, out requestBuilder, out requestRunner);

            IDictionary<string, object> properties;
            IDictionary<string, object> associationsByValue;
            IDictionary<string, int> associationsByContentId;
            VerifyEntryData(requestBuilder, tableName, allData, out properties, out associationsByValue, out associationsByContentId);

            var entry = DataServicesHelper.CreateDataElement(properties);
            foreach (var association in associationsByValue)
            {
                CreateLink(entry, tableName, association);
            }

            var commandText = GetTableActualName(tableName) + "(" + keys + ")";
            var command = new HttpCommand(merge ? RestVerbs.MERGE : RestVerbs.PUT, commandText, updatedData, entry.ToString());
            requestBuilder.AddCommandToRequest(command);
            var result = requestRunner.UpdateEntry(command);

            foreach (var association in associationsByContentId)
            {
                var linkCommand = CreateLinkCommand(tableName, association.Key, command.ContentId, association.Value);
                requestBuilder.AddCommandToRequest(linkCommand);
                requestRunner.UpdateEntry(linkCommand);
            }
            
            return result;
        }

        private int DeleteByExpression(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            // TODO: optimize
            string[] keyFieldNames = GetSchema().FindTable(tableName).PrimaryKey.AsEnumerable().ToArray();
            var entries = FindByExpression(tableName, criteria);

            foreach (var entry in entries)
            {
                var namedKeyValues = new Dictionary<string, object>();
                for (int index = 0; index < keyFieldNames.Count(); index++)
                {
                    namedKeyValues.Add(keyFieldNames[index], entry[keyFieldNames[index]]);
                }
                var formattedKeyValues = _expressionFormatter.Format(namedKeyValues);
                DeleteEntry(tableName, formattedKeyValues, transaction);
            }
            // TODO: what to return?
            return 0;
        }

        private int DeleteEntry(string tableName, string keys, IAdapterTransaction transaction)
        {
            RequestBuilder requestBuilder;
            RequestRunner requestRunner;
            GetRequestHandlers(transaction, out requestBuilder, out requestRunner);

            var commandText = GetTableActualName(tableName) + "(" + keys + ")";
            var command = HttpCommand.Delete(commandText);
            requestBuilder.AddCommandToRequest(command);
            return requestRunner.DeleteEntry(command);
        }

        private HttpCommand CreateLinkCommand(string tableName, string associationName, int entryContentId, int linkContentId)
        {
            var linkEntry = DataServicesHelper.CreateLinkElement(linkContentId);
            var linkMethod = GetSchema().FindTable(tableName).FindAssociation(associationName).Multiplicity.Contains("*") ? RestVerbs.POST : RestVerbs.PUT;

            var commandText = string.Format("${0}/$links/{1}", entryContentId, associationName);
            return new HttpCommand(linkMethod, commandText, null, linkEntry.ToString(), true);
        }

        private void CreateLink(XElement entry, string tableName, KeyValuePair<string, object> associatedData)
        {
            var association = GetSchema().FindTable(tableName).FindAssociation(associatedData.Key);
            var linkedContent = associatedData.Value;
            if (association.Multiplicity.Contains("*"))
            {
                var linkedEntries = linkedContent as IEnumerable<object>;
                if (linkedEntries != null)
                {
                    foreach (var linkedEntry in linkedEntries)
                    {
                        LinkEntry(entry, association, linkedEntry);
                    }
                }
            }
            else
            {
                LinkEntry(entry, association, linkedContent);
            }
        }

        private void LinkEntry(XElement entry, Association association, object entryData)
        {
            if (entryData == null)
                return;

            var entryProperties = GetLinkedEntryProperties(entryData);
            var keyFieldNames = GetSchema().FindTable(association.ReferenceTableName).PrimaryKey.AsEnumerable().ToArray();
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

        private string GetTableActualName(string tableName)
        {
            return GetSchema().FindTable(tableName).ActualName;
        }

        private void VerifyEntryData(RequestBuilder requestBuilder, string tableName,
            IDictionary<string, object> data, out IDictionary<string, object> properties,
            out IDictionary<string, object> associationsByValue,
            out IDictionary<string, int> associationsByContentId)
        {
            properties = new Dictionary<string, object>();
            associationsByValue = new Dictionary<string, object>();
            associationsByContentId = new Dictionary<string, int>();

            var table = GetSchema().FindTable(tableName);
            foreach (var item in data)
            {
                if (table.HasColumn(item.Key))
                {
                    properties.Add(item.Key, item.Value);
                }
                else
                {
                    if (table.HasAssociation(item.Key))
                    {
                        int contentId = requestBuilder.GetContentId(item.Value as IDictionary<string, object>);
                        if (contentId == 0)
                        {
                            associationsByValue.Add(item.Key, item.Value);
                        }
                        else
                        {
                            associationsByContentId.Add(item.Key, contentId);
                        }
                    }
                    else
                    {
                        throw new SimpleDataException(string.Format("No property or association found for {0}.", item.Key));
                    }
                }
            }
        }

        private void CheckInsertablePropertiesAreAvailable(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            var table = GetSchema().FindTable(tableName);
            data = data.Where(kvp => table.HasColumn(kvp.Key));

            if (data.Count() == 0)
            {
                throw new SimpleDataException("No properties were found which could be mapped to the database.");
            }
        }

        private void GetRequestHandlers(IAdapterTransaction transaction, out RequestBuilder requestBuilder, out RequestRunner requestRunner)
        {
            requestBuilder = transaction == null
                                     ? new CommandRequestBuilder(_urlBase)
                                     : (transaction as ODataAdapterTransaction).RequestBuilder;
            requestRunner = transaction == null
                                    ? new CommandRequestRunner()
                                    : (transaction as ODataAdapterTransaction).RequestRunner;
        }
    }
}
