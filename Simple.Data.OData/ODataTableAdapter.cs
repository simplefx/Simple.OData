using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;
using Simple.OData.Schema;

namespace Simple.Data.OData
{
    using System.ComponentModel.Composition;
    using Simple.OData;

    [Export("OData", typeof(Adapter))]
    public class ODataTableAdapter : Adapter
    {
        private RequestBuilder _requestBuilder;
        private ExpressionFormatter _expressionFormatter;
        private DatabaseSchema _schema;

        internal DatabaseSchema GetSchema()
        {
            return _schema ?? (_schema = DatabaseSchema.Get(_requestBuilder));
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            _requestBuilder = new RequestBuilder { UrlBase = Settings.Url };
            _expressionFormatter = new ExpressionFormatter(DatabaseSchema.Get(_requestBuilder).FindTable);
            _schema = DatabaseSchema.Get(_requestBuilder);
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return new ODataTable(tableName, _requestBuilder).Query(criteria);
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
//            return new Finder(_requestBuilder, _expressionFormatter).Get(tableName, keyValues);
            return new ODataTable(tableName, _requestBuilder).Get(keyValues);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return new ODataTable(query.TableName, _requestBuilder).Query(query, out unhandledClauses);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            CheckInsertablePropertiesAreAvailable(tableName, data);
            var table = new ODataTable(tableName, _requestBuilder);
            return table.Insert(data, resultRequired);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            // TODO: optimize
            var table = new ODataTable(tableName, _requestBuilder);
            string[] keyFieldNames = _schema.FindTable(tableName).PrimaryKey.AsEnumerable().ToArray();
            var entries = table.Query(criteria);

            foreach (var entry in entries)
            {
                foreach (var kv in data)
                {
                    entry[kv.Key] = kv.Value;
                }
                var namedKeyValues = new Dictionary<string, object>();
                for (int index = 0; index < keyFieldNames.Count(); index++)
                {
                    namedKeyValues.Add(keyFieldNames[index], entry[keyFieldNames[index]]);
                }
                var formattedKeyValues = _expressionFormatter.Format(namedKeyValues);
                table.Update(formattedKeyValues, entry);
            }
            // TODO: what to return?
            return 0;
        }

        public override int Update(string tableName, IDictionary<string, object> data)
        {
            string[] keyFieldNames = _schema.FindTable(tableName).PrimaryKey.AsEnumerable().ToArray();
            if (keyFieldNames.Length == 0) 
                throw new ODataAdapterException("No Primary Key found for implicit update");
            return Update(tableName, data, GetCriteria(tableName, keyFieldNames, data));
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            // TODO: optimize
            var table = new ODataTable(tableName, _requestBuilder);
            string[] keyFieldNames = _schema.FindTable(tableName).PrimaryKey.AsEnumerable().ToArray();
            var entries = table.Query(criteria);

            foreach (var entry in entries)
            {
                var namedKeyValues = new Dictionary<string, object>();
                for (int index = 0; index < keyFieldNames.Count(); index++)
                {
                    namedKeyValues.Add(keyFieldNames[index], entry[keyFieldNames[index]]);
                }
                var formattedKeyValues = _expressionFormatter.Format(namedKeyValues);
                table.Delete(formattedKeyValues);
            }
            // TODO: what to return?
            return 0;
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return functionName.Equals("like", StringComparison.OrdinalIgnoreCase)
                && args.Length == 1
                && args[0] is string;
        }

        private void CheckInsertablePropertiesAreAvailable(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            var table = DatabaseSchema.Get(_requestBuilder).FindTable(tableName);
            data = data.Where(kvp => table.HasColumn(kvp.Key));

            if (data.Count() == 0)
            {
                throw new SimpleDataException("No properties were found which could be mapped to the database.");
            }
        }
    }
}
