using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Simple.Data;

namespace Simple.OData
{
    public class QueryBuilder
    {
        public static QueryBuilder BuildFrom(SimpleQuery query)
        {
            var builder = new QueryBuilder();
            builder.Build(query);
            return builder;
        }

        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, bool>> _processors;
        private readonly List<SimpleReference> _columns;
        private readonly List<SimpleOrderByItem> _order;
        private Stack<SimpleQueryClauseBase> _processedClauses;

        public IEnumerable<SimpleReference> Columns
        {
            get { return _columns; }
        }

        public SimpleExpression Criteria { get; private set; }

        public bool IsTotalCountQuery { get; private set; }

        public IEnumerable<SimpleOrderByItem> Order
        {
            get { return _order; }
        }

        public int? SkipCount { get; private set; }

        public int? TakeCount { get; private set; }

        public Action<int> SetTotalCount { get; private set; }

        public IEnumerable<SimpleQueryClauseBase> UnprocessedClauses { get; private set; }

        public QueryBuilder()
        {
            _columns = new List<SimpleReference>();
            _order = new List<SimpleOrderByItem>();
            _processedClauses = new Stack<SimpleQueryClauseBase>();

            _processors = new Dictionary<Type, Func<SimpleQueryClauseBase, bool>>
            {
                { typeof(OrderByClause), c => TryApplyOrderByClause((OrderByClause)c) },
                { typeof(SelectClause), c => TryApplySelectClause((SelectClause)c) },
                { typeof(SkipClause), c => TryApplySkipClause((SkipClause)c) },
                { typeof(TakeClause), c => TryApplyTakeClause((TakeClause)c) },
                { typeof(WhereClause), c => TryApplyWhereClause((WhereClause)c) },
                { typeof(WithCountClause), c => TryApplyWithCountClause((WithCountClause)c) },
            };
        }

        private void Build(SimpleQuery query)
        {
            var unprocessedClauses = new Queue<SimpleQueryClauseBase>(query.Clauses);
            Func<SimpleQueryClauseBase, bool> processor;
            while (unprocessedClauses.Count > 0)
            {
                var clause = unprocessedClauses.Peek();
                var clauseType = clause.GetType();

                if (!_processors.TryGetValue(clauseType, out processor) || !processor(clause))
                    break;

                _processedClauses.Push(unprocessedClauses.Dequeue());
            }

            UnprocessedClauses = unprocessedClauses;
        }

        private bool TryApplyOrderByClause(OrderByClause clause)
        {
            if (SkipCount.HasValue || TakeCount.HasValue)
                return false;

            _order.Add(new SimpleOrderByItem(clause.Reference, clause.Direction));
            return true;
        }

        private bool TryApplySelectClause(SelectClause clause)
        {
            if (_columns.Any())
                return false;

            _columns.AddRange(clause.Columns);
            if (_columns.Count == 1 && _columns[0] is CountSpecialReference)
                IsTotalCountQuery = true;
            return true;
        }

        private bool TryApplySkipClause(SkipClause clause)
        {
            if (SkipCount.HasValue || TakeCount.HasValue)
                return false;

            SkipCount = clause.Count;
            return true;
        }

        private bool TryApplyTakeClause(TakeClause clause)
        {
            if (TakeCount.HasValue)
                return false;

            TakeCount = clause.Count;
            return true;
        }

        private bool TryApplyWhereClause(WhereClause clause)
        {
            if (SkipCount.HasValue || TakeCount.HasValue)
                return false;

            if (_processedClauses.Any() && !(_processedClauses.Peek() is WhereClause))
                return false;

            Criteria = Criteria == null
                ? clause.Criteria
                : new SimpleExpression(Criteria, clause.Criteria, SimpleExpressionType.And);
            return true;
        }

        private bool TryApplyWithCountClause(WithCountClause clause)
        {
            IsTotalCountQuery = true;
            SetTotalCount = clause.SetCount;
            return true;
        }
    }
}
