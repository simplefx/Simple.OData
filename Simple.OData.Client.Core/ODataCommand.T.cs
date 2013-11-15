using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class ODataCommand<T> : ODataCommand
        where T : class
    {
        public ODataCommand(ISchema schema, ODataCommand parent)
            : base(schema, parent)
        {
        }

        internal ODataCommand(ODataCommand ancestor)
            : base(ancestor)
        {
        }

        public void Key(T entryKey)
        {
            base.Key(entryKey.ToDictionary());
        }

        public void Filter(Expression<Func<T, bool>> expression)
        {
            base.Filter(ODataExpression.FromLinqExpression(expression.Body));
        }

        public void Expand(Expression<Func<T, object>> expression)
        {
            base.Expand(ExtractColumnNames(expression));
        }

        public void Select(Expression<Func<T, object>> expression)
        {
            base.Select(ExtractColumnNames(expression));
        }

        public void OrderBy(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
        }

        public void ThenBy(Expression<Func<T, object>> expression)
        {
            base.ThenBy(ExtractColumnNames(expression).ToArray());
        }

        public void OrderByDescending(Expression<Func<T, object>> expression)
        {
            base.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
        }

        public void ThenByDescending(Expression<Func<T, object>> expression)
        {
            base.ThenByDescending(ExtractColumnNames(expression).ToArray());
        }

        public void Set(T entry)
        {
            base.Set(entry.ToDictionary());
        }
    }
}