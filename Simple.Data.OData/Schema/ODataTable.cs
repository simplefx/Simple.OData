using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.OData;
using Simple.OData.Schema;

namespace Simple.Data.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class ODataTable : Table
    {
        private readonly RequestBuilder _requestBuilder;
        private readonly DatabaseSchema _databaseSchema;

        public ODataTable(string name, RequestBuilder requestBuilder)
            : base(name)
        {
            _requestBuilder = requestBuilder;
            _databaseSchema = DatabaseSchema.Get(_requestBuilder);
        }

        internal ODataTable(string name, RequestBuilder requestBuilder, DatabaseSchema databaseSchema)
            : base(name)
        {
            _requestBuilder = requestBuilder;
            _databaseSchema = databaseSchema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_databaseSchema.SchemaProvider.GetColumns(this));
        }

        private Key GetPrimaryKey()
        {
            return _databaseSchema.SchemaProvider.GetPrimaryKey(this);
        }

        public IEnumerable<IDictionary<string, object>> GetAllRows()
        {
            return Find(_actualName);
        }

        public IEnumerable<IDictionary<string, object>> Query(SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter(_databaseSchema.FindTable).Format(criteria);
            var command = new CommandBuilder(_databaseSchema.FindTable).BuildCommand(_actualName, filter);
            return Find(command);
        }

        public IEnumerable<IDictionary<string, object>> Query(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = new CommandBuilder(DatabaseSchema.Get(_requestBuilder).FindTable);
            string command = builder.BuildCommand(query);
            unhandledClauses = builder.UnprocessedClauses;
            var results = Find(command, builder.IsScalarResult);

            return results;

            //if (builder.IsTotalCountQuery)
            //    return new List<IDictionary<string, object>> { (new Dictionary<string, object> { { "count", results.Count() } }) };
            //else
            //    return results;
        }

        public IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return DatabaseSchema.Get(_requestBuilder).FindTable(_actualName).PrimaryKey.AsEnumerable().ToList();
        }

        public IDictionary<string, object> Get(object[] keyValues)
        {
            var keyNames = GetKeyNames();
            var namedKeyValues = new Dictionary<string, object>();
            for (int index = 0; index < keyValues.Count(); index++)
            {
                namedKeyValues.Add(keyNames[index], keyValues[index]);
            }
            var formattedKeyValues = new ExpressionFormatter(_databaseSchema.FindTable).Format(namedKeyValues);
            return Get(formattedKeyValues).FirstOrDefault();
        }

        public IEnumerable<IDictionary<string, object>> Get(string keys)
        {
            return Find(_databaseSchema.FindTable(_actualName).ActualName + "(" + keys + ")");
        }

        internal IDictionary<string, object> Insert(IDictionary<string, object> data, bool resultRequired)
        {
            var url = _databaseSchema.FindTable(_actualName).ActualName;
            var entry = DataServicesHelper.CreateDataElement(data);
            var request = _requestBuilder.CreateTableRequest(url, RestVerbs.POST, entry.ToString());

            var text = new RequestRunner().Request(request);
            if (resultRequired)
            {
                return DataServicesHelper.GetData(text).First();
            }
            else
            {
                return null;
            }
        }

        public int Update(string keys, IDictionary<string, object> data)
        {
            var url = _databaseSchema.FindTable(_actualName).ActualName + "(" + keys + ")";
            var entry = DataServicesHelper.CreateDataElement(data);
            var request = _requestBuilder.CreateTableRequest(url, RestVerbs.PUT, entry.ToString());

            using (var response = new RequestRunner().TryRequest(request))
            {
                // TODO
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        public int Delete(string keys)
        {
            var url = _databaseSchema.FindTable(_actualName).ActualName + "(" + keys + ")";
            var request = _requestBuilder.CreateTableRequest(url, RestVerbs.DELETE);

            using (var response = new RequestRunner().TryRequest(request))
            {
                // TODO: check response code
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        private IEnumerable<IDictionary<string, object>> Find(string url, bool scalarResult = false)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _requestBuilder.CreateTableRequest(url, RestVerbs.GET);

            using (var response = new RequestRunner().TryRequest(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    result = DataServicesHelper.GetData(response.GetResponseStream(), scalarResult);
                }
            }

            return result;
        }
    }
}
