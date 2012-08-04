using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    class ODataAdapterTransaction : IAdapterTransaction
    {
        private readonly ODataTableAdapter _adapter;
        private List<Tuple<string, object[]>> _calls; 

        public ODataAdapterTransaction(ODataTableAdapter adapter)
        {
            _adapter = adapter;
            _calls = new List<Tuple<string, object[]>>();
        }

        public string Name
        {
            get { return null; }
        }

        public void Commit()
        {
            foreach (var call in _calls)
            {
                switch (call.Item1)
                {
                    case "Delete":
                        _adapter.Delete((string)call.Item2[0], (SimpleExpression)call.Item2[1]);
                        break;
                    case "Insert":
                        _adapter.Insert((string)call.Item2[0], (IDictionary<string, object>)call.Item2[1], (bool)call.Item2[2]);
                        break;
                    case "InsertMany":
                        _adapter.InsertMany((string)call.Item2[0], (IEnumerable<IDictionary<string, object>>)call.Item2[1], (Func<IDictionary<string, object>, Exception, bool>)call.Item2[2], (bool)call.Item2[3]);
                        break;
                    case "Update":
                        _adapter.Update((string)call.Item2[0], (IDictionary<string, object>)call.Item2[1], (SimpleExpression)call.Item2[2]);
                        break;
                    case "UpdateMany":
                        switch (call.Item2.Length)
                        {
                            case 2:
                                _adapter.UpdateMany((string)call.Item2[0], (IEnumerable<IDictionary<string, object>>)call.Item2[1]);
                                break;
                            case 3:
                                if (call.Item2[1] is IList<IDictionary<string, object>>)
                                    _adapter.UpdateMany((string)call.Item2[0], (IList<IDictionary<string, object>>)call.Item2[1], (IEnumerable<string>)call.Item2[2]);
                                else
                                    _adapter.UpdateMany((string)call.Item2[0], (IEnumerable<IDictionary<string, object>>)call.Item2[1], (IList<string>)call.Item2[2]);
                                break;
                        }
                        break;
                }
            }
            _calls.Clear();
        }

        public void Rollback()
        {
            _calls.Clear();
        }

        public void Dispose()
        {
            _calls.Clear();
        }

        public int Delete(string tableName, SimpleExpression criteria)
        {
            _calls.Add(new Tuple<string, object[]>("Delete", new object[] { tableName, criteria }));
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
            _calls.Add(new Tuple<string, object[]>("Insert", new object[] { tableName, data, resultRequired }));
            return null;
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            _calls.Add(new Tuple<string, object[]>("InsertMany", new object[] { tableName, data, onError, resultRequired }));
            return null;
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return _adapter.RunQuery(query, out unhandledClauses);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            _calls.Add(new Tuple<string, object[]>("Update", new object[] { tableName, data, criteria }));
            return 0;
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            _calls.Add(new Tuple<string, object[]>("UpdateMany", new object[] { tableName, dataList, criteriaFieldNames }));
            return 0;
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IList<string> keyFields)
        {
            _calls.Add(new Tuple<string, object[]>("UpdateMany", new object[] { tableName, dataList, keyFields }));
            return 0;
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList)
        {
            _calls.Add(new Tuple<string, object[]>("UpdateMany", new object[] { tableName, dataList }));
            return 0;
        }
    }
}
