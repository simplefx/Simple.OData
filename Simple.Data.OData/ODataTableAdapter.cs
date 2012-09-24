using System;
using System.Collections.Generic;
using System.Linq;
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
            return GetTable(tableName).GetKey(tableName, record);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            return GetTable(tableName).GetKeyNames();
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
            return new RequestExecutor(_urlBase, _schema).InsertEntry(tableName, data, null, resultRequired);
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
            var commandText = builder.BuildCommand(tableName, filter);

            return FindEntries(commandText);
        }

        private IEnumerable<IDictionary<string, object>> FindByQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = new CommandBuilder(GetSchema().FindTable);
            var command = builder.BuildCommand(query, GetKeyNames);
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
            var builder = new CommandBuilder(GetSchema().FindTable);
            var command = builder.BuildCommand(tableName, keyValues, GetKeyNames);
            return FindEntries(command).SingleOrDefault();
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult = false)
        {
            int totalCount;
            return new RequestExecutor(_urlBase, _schema).FindEntries(commandText, scalarResult, false, out totalCount);
        }

        private IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount)
        {
            return new RequestExecutor(_urlBase, _schema).FindEntries(commandText, false, true, out totalCount);
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
                new RequestExecutor(_urlBase, _schema, transaction).UpdateEntry(tableName, formattedKeyValues, data, merge, transaction);
            }
            // TODO: what to return?
            return 0;
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
                new RequestExecutor(_urlBase, _schema, transaction).DeleteEntry(tableName, formattedKeyValues, transaction);
            }
            // TODO: what to return?
            return 0;
        }

        private Table GetTable(string tableName)
        {
            return new Table(tableName, GetSchema());
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
