using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using System.ComponentModel.Composition;
    using Simple.OData;

    [Export("Azure", typeof(Adapter))]
    public class AzureTableAdapter : Adapter
    {
        private RequestBuilder _helper;

        protected override void OnSetup()
        {
            base.OnSetup();
            _helper = new RequestBuilder { UrlBase = Settings.Url, SharedKey = Settings.Key, Account = Settings.Account };
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            // TODO: initialize expression formatter with FindTable delegate
            var filter = new ExpressionFormatter(null).Format(criteria);
            var table = new AzureTable(tableName, _helper);
            return table.Query(filter);
        }

        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            throw new NotImplementedException();
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            throw new NotImplementedException();
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
