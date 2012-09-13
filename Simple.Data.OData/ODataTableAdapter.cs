using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            var command = builder.BuildCommand(GetTableActualName(tableName), filter);

            return FindEntries(command);
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

        private IEnumerable<IDictionary<string, object>> FindEntries(string url, bool scalarResult = false)
        {
            int totalCount;
            return FindEntries(url, scalarResult, false, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string url, out int totalCount)
        {
            return FindEntries(url, false, true, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string url, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            var requestBuilder = new CommandRequestBuilder(_urlBase);
            requestBuilder.AddTableCommand(url, RestVerbs.GET);
            return new CommandRequestRunner(requestBuilder).FindEntries(scalarResult, setTotalCount, out totalCount);
        }

        private IDictionary<string, object> InsertEntry(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            RequestBuilder requestBuilder;
            RequestRunner requestRunner;
            GetRequestHandlers(transaction, out requestBuilder, out requestRunner);

            IDictionary<string, object> properties;
            IDictionary<string, object> associations;
            VerifyEntryData(tableName, data, out properties, out associations);

            var entry = DataServicesHelper.CreateDataElement(properties);
            foreach (var association in associations)
            {
                LinkEntry(entry, tableName, association);
            }

            var command = GetTableActualName(tableName);
            requestBuilder.AddTableCommand(command, RestVerbs.POST, entry.ToString());
            var result = requestRunner.InsertEntry(resultRequired);
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
                foreach (var item in entry)
                {
                    if (!formattedKeyValues.Contains(item.Key) && !data.ContainsKey(item.Key))
                    {
                        unaffectedData.Add(item.Key, item.Value);
                    }
                }
                UpdateEntry(tableName, formattedKeyValues, data, unaffectedData, transaction);
            }
            // TODO: what to return?
            return 0;
        }

        private int UpdateEntry(string tableName, string keys, IDictionary<string, object> updatedData, IDictionary<string, object> unaffectedData, IAdapterTransaction transaction)
        {
            RequestBuilder requestBuilder;
            RequestRunner requestRunner;
            GetRequestHandlers(transaction, out requestBuilder, out requestRunner);

            Dictionary<string, object> allData = new Dictionary<string, object>();
            updatedData.Keys.ToList().ForEach(x => allData.Add(x, updatedData[x]));
            unaffectedData.Keys.ToList().ForEach(x => allData.Add(x, unaffectedData[x]));

            IDictionary<string, object> properties;
            IDictionary<string, object> associations;
            VerifyEntryData(tableName, allData, out properties, out associations);

            var entry = DataServicesHelper.CreateDataElement(properties);
            foreach (var association in associations)
            {
                LinkEntry(entry, tableName, association);
            }

            var command = GetTableActualName(tableName) + "(" + keys + ")";
            requestBuilder.AddTableCommand(command, RestVerbs.PUT, entry.ToString());
            var result = requestRunner.UpdateEntry();
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

            var command = GetTableActualName(tableName) + "(" + keys + ")";
            requestBuilder.AddTableCommand(command, RestVerbs.DELETE);
            return requestRunner.DeleteEntry();
        }

        private void LinkEntry(XElement entry, string tableName, KeyValuePair<string, object> linkedEntry)
        {
            var association = GetSchema().FindTable(tableName).FindAssociation(linkedEntry.Key);
            var entryProperties = linkedEntry.Value as IDictionary<string, object>;
            var keyFieldNames = GetSchema().FindTable(association.ReferenceTableName).PrimaryKey.AsEnumerable().ToArray();
            var keyFieldValues = new object[keyFieldNames.Count()];
            for (int index = 0; index < keyFieldNames.Count(); index++)
            {
                keyFieldValues[index] = entryProperties[keyFieldNames[index]];
            }
            DataServicesHelper.AddDataLink(entry, association.ActualName, association.ReferenceTableName, keyFieldValues);
        }

        private string GetTableActualName(string tableName)
        {
            return GetSchema().FindTable(tableName).ActualName;
        }

        private void VerifyEntryData(string tableName, IDictionary<string, object> data, out IDictionary<string, object> properties, out IDictionary<string, object> associations)
        {
            properties = new Dictionary<string, object>();
            associations = new Dictionary<string, object>();
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
                        associations.Add(item.Key, item.Value);
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
                                    ? new CommandRequestRunner(requestBuilder)
                                    : (transaction as ODataAdapterTransaction).RequestRunner;
        }
    }
}
