using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    using Simple.OData;
    using Simple.Data.OData.Helpers;

    public class Finder
    {
        private ODataHelper _helper;
        private ExpressionFormatter _expressionFormatter;

        public Finder(ODataHelper helper, ExpressionFormatter expressionFormatter)
        {
            _helper = helper;
            _expressionFormatter = expressionFormatter;
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter().Format(criteria);
            var table = new Table(tableName, _helper);
            return table.Query(filter);
        }

        public IEnumerable<IDictionary<string, object>> Find(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = QueryBuilder.BuildFrom(query);

            var table = new Table(query.TableName, _helper);
            unhandledClauses = builder.UnprocessedClauses;
            var results = table.Query(_expressionFormatter.Format(builder.Criteria));

            if (builder.IsTotalCountQuery)
                return new List<IDictionary<string, object>> { (new Dictionary<string, object> {{"COUNT", results.Count()}}) };
            else
                return results;
        }
    }
}
