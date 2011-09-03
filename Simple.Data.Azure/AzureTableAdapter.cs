using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using System.ComponentModel.Composition;
    using Helpers;

    [Export("Azure", typeof(Adapter))]
    public class AzureTableAdapter : Adapter
    {
        private AzureHelper _helper;

        protected override void OnSetup()
        {
            base.OnSetup();
            _helper = new AzureHelper { UrlBase = Settings.Url, SharedKey = Settings.Key, Account = Settings.Account };
        }
        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter().Format(criteria);
            var table = new Table(tableName, _helper);
            return table.Query(filter);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public override int Update(string tableName, IDictionary<string, object> data)
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
