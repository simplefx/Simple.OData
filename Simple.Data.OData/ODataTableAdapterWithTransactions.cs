using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    public partial class ODataTableAdapter : IAdapterWithTransactions
    {
        public IAdapterTransaction BeginTransaction(string name, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
        {
            return BeginTransaction();
        }

        public IAdapterTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
        {
            return new ODataAdapterTransaction(this);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return this.Find(tableName, criteria);
        }

        public IDictionary<string, object> Get(string tableName, IAdapterTransaction transaction, params object[] parameterValues)
        {
            return this.Get(tableName, parameterValues);
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return this.RunQuery(query, out unhandledClauses);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            CheckInsertablePropertiesAreAvailable(tableName, data);
            return InsertEntry(tableName, data, transaction, resultRequired);
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, IAdapterTransaction transaction, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            return this.InsertMany(tableName, data, onError, resultRequired);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return UpdateByExpression(tableName, data, criteria, transaction);
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction transaction)
        {
            return this.UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction transaction, IList<string> keyFields)
        {
            return this.UpdateMany(tableName, dataList, keyFields);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction transaction)
        {
            return this.UpdateMany(tableName, dataList);
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return DeleteByExpression(tableName, criteria, transaction);
        }
    }
}
