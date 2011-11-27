using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
{
    using Simple.OData;
    using Simple.Data.OData.Helpers;

    public class Finder
    {
        private ProviderHelper _helper;
        private ExpressionFormatter _expressionFormatter;

        public Finder(ProviderHelper helper, ExpressionFormatter expressionFormatter)
        {
            _helper = helper;
            _expressionFormatter = expressionFormatter;
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter().Format(criteria);
            var table = new Table(tableName, _helper);
            return table.QueryWithFilter(filter);
        }

        public IEnumerable<IDictionary<string, object>> Find(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = QueryBuilder.BuildFrom(query);

            var table = new Table(query.TableName, _helper);
            unhandledClauses = builder.UnprocessedClauses;
            var results = table.QueryWithFilter(_expressionFormatter.Format(builder.Criteria));

            if (builder.IsTotalCountQuery)
                return new List<IDictionary<string, object>> { (new Dictionary<string, object> {{"COUNT", results.Count()}}) };
            else
                return results;
        }

        internal IDictionary<string, object> Get(string tableName, object[] keyValues)
        {
            var formattedKeyValues = new ExpressionFormatter().Format(keyValues);
            var table = new Table(tableName, _helper);
            return table.QueryWithKeys(formattedKeyValues).FirstOrDefault();
        }
    }
}
