using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    using System.ComponentModel.Composition;
    using Simple.OData;
    using Helpers;

    [Export("OData", typeof(Adapter))]
    public class ODataTableAdapter : Adapter
    {
        private ODataHelper _helper;
        private ExpressionFormatter _expressionFormatter;

        protected override void OnSetup()
        {
            base.OnSetup();

            _helper = new ODataHelper { UrlBase = Settings.Url };
            _expressionFormatter = new ExpressionFormatter();
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return new Finder(_helper, _expressionFormatter).Find(tableName, criteria);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return new Finder(_helper, _expressionFormatter).Find(query, out unhandledClauses);
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
            return functionName.Equals("like", StringComparison.OrdinalIgnoreCase)
                && args.Length == 1
                && args[0] is string;
        }
    }
}
