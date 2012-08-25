using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData;

namespace Simple.Data.OData
{
    class ODataAdapterTransaction : IAdapterTransaction
    {
        class AdapterRequest
        {
            public string MethodName { get; private set; }
            public object[] Parameters { get; private set; }

            public AdapterRequest(string methodName, params object[] parameters)
            {
                this.MethodName = methodName;
                this.Parameters = parameters;
            }
        }

        private readonly ODataTableAdapter _adapter;
        private List<AdapterRequest> _requests;

        public ODataAdapterTransaction(ODataTableAdapter adapter)
        {
            _adapter = adapter;
            _adapter.SetRequestHandlers(new BatchRequestBuilder(_adapter.UrlBase), new RequestRunner());
            _requests = new List<AdapterRequest>();
        }

        public string Name
        {
            get { return null; }
        }

        public void Commit()
        {
            SendBatchRequest();
            _adapter.SetRequestHandlers(new CommandRequestBuilder(_adapter.UrlBase), new RequestRunner());
            _requests.Clear();
        }

        public void Rollback()
        {
            _adapter.SetRequestHandlers(new CommandRequestBuilder(_adapter.UrlBase), new RequestRunner());
            _requests.Clear();
        }

        public void Dispose()
        {
            _requests.Clear();
        }

        public int Delete(string tableName, SimpleExpression criteria)
        {
            _requests.Add(new AdapterRequest("Delete", tableName, criteria ));
            return 0;
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria);
        }

        public IDictionary<string, object> Get(string tableName, params object[] parameterValues)
        {
            return _adapter.Get(tableName, parameterValues);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            _requests.Add(new AdapterRequest("Insert", tableName, data, resultRequired ));
            return null;
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            _requests.Add(new AdapterRequest("InsertMany", tableName, data, onError, resultRequired ));
            return null;
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return _adapter.RunQuery(query, out unhandledClauses);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            _requests.Add(new AdapterRequest("Update", tableName, data, criteria ));
            return 0;
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            _requests.Add(new AdapterRequest("UpdateMany1", tableName, dataList, criteriaFieldNames ));
            return 0;
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IList<string> keyFields)
        {
            _requests.Add(new AdapterRequest("UpdateMany2", tableName, dataList, keyFields ));
            return 0;
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList)
        {
            _requests.Add(new AdapterRequest("UpdateMany3", tableName, dataList ));
            return 0;
        }

        private void SendBatchRequest()
        {
            foreach (var request in _requests)
            {
                var tableName = (string)request.Parameters[0];
                switch (request.MethodName)
                {
                    case "Delete":
                        _adapter.Delete(tableName, 
                            (SimpleExpression)request.Parameters[1]);
                        break;
                    case "Insert":
                        _adapter.Insert(tableName, 
                            (IDictionary<string, object>)request.Parameters[1], (bool)request.Parameters[2]);
                        break;
                    case "InsertMany":
                        _adapter.InsertMany(tableName, 
                            (IEnumerable<IDictionary<string, object>>)request.Parameters[1], (Func<IDictionary<string, object>, Exception, bool>)request.Parameters[2], (bool)request.Parameters[3]);
                        break;
                    case "Update":
                        _adapter.Update(tableName, 
                            (IDictionary<string, object>)request.Parameters[1], (SimpleExpression)request.Parameters[2]);
                        break;
                    case "UpdateMany1":
                        _adapter.UpdateMany(tableName, 
                            (IList<IDictionary<string, object>>)request.Parameters[1], (IEnumerable<string>)request.Parameters[2]);
                        break;
                    case "UpdateMany2":
                        _adapter.UpdateMany(tableName, 
                            (IEnumerable<IDictionary<string, object>>)request.Parameters[1], (IList<string>)request.Parameters[2]);
                        break;
                    case "UpdateMany3":
                        _adapter.UpdateMany(tableName, 
                            (IEnumerable<IDictionary<string, object>>)request.Parameters[1]);
                        break;
                }
            }
        }
    }
}
