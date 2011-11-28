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
        private ProviderHelper _providerHelper;
        private ExpressionFormatter _expressionFormatter;

        public Finder(ProviderHelper providerHelper, ExpressionFormatter expressionFormatter)
        {
            _providerHelper = providerHelper;
            _expressionFormatter = expressionFormatter;
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var filter = new ExpressionFormatter().Format(criteria);
            var table = new Table(tableName, _providerHelper);
            return table.QueryWithFilter(filter);
        }

        public IEnumerable<IDictionary<string, object>> Find(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var builder = QueryBuilder.BuildFrom(query);

            var table = new Table(query.TableName, _providerHelper);
            unhandledClauses = builder.UnprocessedClauses;
            var results = table.QueryWithFilter(_expressionFormatter.Format(builder.Criteria));

            if (builder.IsTotalCountQuery)
                return new List<IDictionary<string, object>> { (new Dictionary<string, object> {{"COUNT", results.Count()}}) };
            else
                return results;
        }

        internal IDictionary<string, object> Get(string tableName, object[] keyValues)
        {
            var key = DatabaseSchema.Get(_providerHelper).FindTable(tableName).PrimaryKey;
            var namedKeyValues = new Dictionary<string, object>();
            for (int index = 0; index < keyValues.Count(); index++)
            {
                namedKeyValues.Add(key[index], keyValues[index]);
            }
            var formattedKeyValues = new ExpressionFormatter().Format(namedKeyValues);
            var table = new Table(tableName, _providerHelper);
            return table.QueryWithKeys(formattedKeyValues).FirstOrDefault();
        }
    }
}
