using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        private RequestBuilder _requestBuilder;
        private RequestRunner _requestRunner ;

        internal string UrlBase
        {
            get { return _urlBase; }
        }

        internal DatabaseSchema GetSchema()
        {
            return _schema ?? (_schema = DatabaseSchema.Get(_urlBase));
        }

        internal void SetRequestHandlers(RequestBuilder requestBuilder, RequestRunner requestRunner)
        {
            _requestBuilder = requestBuilder;
            _requestRunner = requestRunner;
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            _urlBase = Settings.Url;
            _expressionFormatter = new ExpressionFormatter(DatabaseSchema.Get(_urlBase).FindTable);
            _schema = DatabaseSchema.Get(_urlBase);
            _requestBuilder = new CommandRequestBuilder(_urlBase);
            _requestRunner = new RequestRunner();
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
            return InsertEntry(tableName, data, resultRequired);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
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
                UpdateEntry(tableName, formattedKeyValues, data);
            }
            // TODO: what to return?
            return 0;
        }

        public override int Delete(string tableName, SimpleExpression criteria)
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
                DeleteEntry(tableName, formattedKeyValues);
            }
            // TODO: what to return?
            return 0;
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
            IEnumerable<IDictionary<string, object>> result;
            _requestBuilder.AddTableCommand(url, RestVerbs.GET);
            totalCount = 0;

            using (var response = _requestRunner.TryRequest(_requestBuilder.Request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    var stream = response.GetResponseStream();
                    if (setTotalCount)
                        result = DataServicesHelper.GetData(stream, out totalCount);
                    else
                        result = DataServicesHelper.GetData(response.GetResponseStream(), scalarResult);
                }

                return result;
            }
        }

        private IDictionary<string, object> InsertEntry(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            var entry = DataServicesHelper.CreateDataElement(data);
            var command = GetTableActualName(tableName);
            _requestBuilder.AddTableCommand(command, RestVerbs.POST, entry.ToString());

            var text = _requestRunner.Request(_requestBuilder.Request);
            if (resultRequired)
            {
                return DataServicesHelper.GetData(text).First();
            }
            else
            {
                return null;
            }
        }

        private int UpdateEntry(string tableName, string keys, IDictionary<string, object> data)
        {
            var entry = DataServicesHelper.CreateDataElement(data);
            var command = GetTableActualName(tableName) + "(" + keys + ")"; 
            _requestBuilder.AddTableCommand(command, RestVerbs.PUT, entry.ToString());

            using (var response = _requestRunner.TryRequest(_requestBuilder.Request))
            {
                // TODO
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        private int DeleteEntry(string tableName, string keys)
        {
            var command = GetTableActualName(tableName) + "(" + keys + ")";
            _requestBuilder.AddTableCommand(command, RestVerbs.DELETE);

            using (var response = _requestRunner.TryRequest(_requestBuilder.Request))
            {
                // TODO: check response code
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        private string GetTableActualName(string tableName)
        {
            return GetSchema().FindTable(tableName).ActualName;
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
    }
}
