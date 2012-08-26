using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using Simple.Data.OData;
using Simple.Data.OData.Schema;

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
            _lazyAssociations = new Lazy<AssociationCollection>(GetAssociations);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_databaseSchema.SchemaProvider.GetColumns(this));
        }

        private AssociationCollection GetAssociations()
        {
            return new AssociationCollection(_databaseSchema.SchemaProvider.GetAssociations(this));
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
            var builder = new CommandBuilder(_databaseSchema.FindTable);
            var command = builder.BuildCommand(_actualName, filter);

            return Find(command);
        }

        public IEnumerable<IDictionary<string, object>> Query(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = new CommandBuilder(DatabaseSchema.Get(_requestBuilder).FindTable);
            string command = builder.BuildCommand(query);
            unhandledClauses = builder.UnprocessedClauses;
            IEnumerable<IDictionary<string, object>> results;

            if (builder.SetTotalCount == null)
            {
                results = Find(command, builder.IsScalarResult);
            }
            else
            {
                int totalCount;
                results = Find(command, out totalCount);
                builder.SetTotalCount(totalCount);
            }
            return results;
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
            int totalCount;
            return Find(url, scalarResult, false, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> Find(string url, out int totalCount)
        {
            return Find(url, false, true, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> Find(string url, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _requestBuilder.CreateTableRequest(url, RestVerbs.GET);
            totalCount = 0;

            using (var response = new RequestRunner().TryRequest(request))
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
    }
}
