using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
{
    using System.ComponentModel.Composition;
    using Simple.OData;
    using Helpers;

    [Export("OData", typeof(Adapter))]
    public class ODataTableAdapter : Adapter
    {
        private ProviderHelper _providerHelper;
        private ExpressionFormatter _expressionFormatter;
        private DatabaseSchema _schema;

        internal DatabaseSchema GetSchema()
        {
            return _schema ?? (_schema = DatabaseSchema.Get(_providerHelper));
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            _providerHelper = new ProviderHelper { UrlBase = Settings.Url };
            _expressionFormatter = new ExpressionFormatter();
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return new Finder(_providerHelper, _expressionFormatter).Find(tableName, criteria);
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            return new Finder(_providerHelper, _expressionFormatter).Get(tableName, keyValues);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return new Finder(_providerHelper, _expressionFormatter).Find(query, out unhandledClauses);
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
