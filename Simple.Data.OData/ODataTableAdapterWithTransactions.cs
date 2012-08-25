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
            _requestBuilder = new BatchRequestBuilder(_urlBase);
            return new ODataAdapterTransaction(this);
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return transaction == null? 
                this.Delete(tableName, criteria) : 
                (transaction as ODataAdapterTransaction).Delete(tableName, criteria);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return transaction == null ? 
                this.Find(tableName, criteria) :
                (transaction as ODataAdapterTransaction).Find(tableName, criteria);
        }

        public IDictionary<string, object> Get(string tableName, IAdapterTransaction transaction, params object[] parameterValues)
        {
            return transaction == null ? 
                this.Get(tableName, parameterValues) :
                (transaction as ODataAdapterTransaction).Get(tableName, parameterValues);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            return (transaction as ODataAdapterTransaction).Insert(tableName, data, resultRequired);
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, IAdapterTransaction transaction, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            return transaction == null ? 
                this.InsertMany(tableName, data, onError, resultRequired) :
                (transaction as ODataAdapterTransaction).InsertMany(tableName, data, onError, resultRequired);
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return transaction == null ? 
                this.RunQuery(query, out unhandledClauses) :
                (transaction as ODataAdapterTransaction).RunQuery(query, out unhandledClauses);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return transaction == null ? 
                this.Update(tableName, data, criteria) :
                (transaction as ODataAdapterTransaction).Update(tableName, data, criteria);
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction transaction)
        {
            return transaction == null ? 
                this.UpdateMany(tableName, dataList, criteriaFieldNames) :
                (transaction as ODataAdapterTransaction).UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction transaction, IList<string> keyFields)
        {
            return transaction == null ? 
                this.UpdateMany(tableName, dataList, keyFields) :
                (transaction as ODataAdapterTransaction).UpdateMany(tableName, dataList, keyFields);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction transaction)
        {
            return transaction == null ? 
                this.UpdateMany(tableName, dataList) :
                (transaction as ODataAdapterTransaction).UpdateMany(tableName, dataList);
        }
    }
}
