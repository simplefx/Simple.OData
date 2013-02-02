using System;
using System.Linq;
using System.Collections.Generic;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    class CommandBuilder
    {
        private bool _excludeResourceTypeExpressions;
        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, QueryCommand, bool>> _processors;

        public CommandBuilder(bool excludeResourceTypeExpressions)
        {
            _excludeResourceTypeExpressions = excludeResourceTypeExpressions;
            _processors = new Dictionary<Type, Func<SimpleQueryClauseBase, QueryCommand, bool>>
            {
                { typeof(OrderByClause), (x,y) => TryApplyOrderByClause((OrderByClause)x, y) },
                { typeof(SelectClause), (x,y) => TryApplySelectClause((SelectClause)x, y) },
                { typeof(SkipClause), (x,y) => TryApplySkipClause((SkipClause)x, y) },
                { typeof(TakeClause), (x,y) => TryApplyTakeClause((TakeClause)x, y) },
                { typeof(WhereClause), (x,y) => TryApplyWhereClause((WhereClause)x, y) },
                { typeof(WithClause), (x,y) => TryApplyWithClause((WithClause)x, y) },
                { typeof(WithCountClause), (x,y) => TryApplyWithCountClause((WithCountClause)x, y) },
            };
        }

        public QueryCommand BuildCommand(string tablePath, SimpleExpression criteria)
        {
            return new QueryCommand()
                {
                    TablePath = tablePath,
                    Criteria = criteria,
                    FilterExpression = new ExpressionConverter(_excludeResourceTypeExpressions).ConvertExpression(criteria),
                };
        }

        public QueryCommand BuildCommand(SimpleQuery query)
        {
            var cmd = new QueryCommand();

            var unprocessedClauses = new Queue<SimpleQueryClauseBase>(query.Clauses);
            Func<SimpleQueryClauseBase, QueryCommand, bool> processor;
            while (unprocessedClauses.Count > 0)
            {
                var clause = unprocessedClauses.Peek();
                var clauseType = clause.GetType();

                if (!_processors.TryGetValue(clauseType, out processor) || !processor(clause, cmd))
                    break;

                cmd.ProcessedClauses.Push(unprocessedClauses.Dequeue());
            }

            cmd.TablePath = query.TableName;
            cmd.UnprocessedClauses = unprocessedClauses;
            return cmd;
        }

        public QueryCommand BuildCommand(string tablePath, object[] keyValues)
        {
            return new QueryCommand() { TablePath = tablePath, KeyValues = keyValues.ToList() };
        }

        private bool TryApplyWithClause(WithClause clause, QueryCommand cmd)
        {
            cmd.Expand.Add(clause.ObjectReference.GetName());
            return true;
        }

        private bool TryApplyOrderByClause(OrderByClause clause, QueryCommand cmd)
        {
            if (cmd.SkipCount.HasValue || cmd.TakeCount.HasValue)
                return false;

            cmd.Order.Add(new SimpleOrderByItem(clause.Reference, clause.Direction));
            return true;
        }

        private bool TryApplySelectClause(SelectClause clause, QueryCommand cmd)
        {
            if (cmd.Columns.Any())
                return false;

            cmd.Columns.AddRange(clause.Columns);
            return true;
        }

        private bool TryApplySkipClause(SkipClause clause, QueryCommand cmd)
        {
            if (cmd.SkipCount.HasValue || cmd.TakeCount.HasValue)
                return false;

            cmd.SkipCount = clause.Count;
            return true;
        }

        private bool TryApplyTakeClause(TakeClause clause, QueryCommand cmd)
        {
            if (cmd.TakeCount.HasValue)
                return false;

            cmd.TakeCount = clause.Count;
            return true;
        }

        private bool TryApplyWhereClause(WhereClause clause, QueryCommand cmd)
        {
            if (cmd.SkipCount.HasValue || cmd.TakeCount.HasValue)
                return false;

            cmd.Criteria = cmd.Criteria == null
                ? clause.Criteria
                : new SimpleExpression(cmd.Criteria, clause.Criteria, SimpleExpressionType.And);

            cmd.FilterExpression = new ExpressionConverter(_excludeResourceTypeExpressions).ConvertExpression(cmd.Criteria);

            return true;
        }

        private bool TryApplyWithCountClause(WithCountClause clause, QueryCommand cmd)
        {
            cmd.SetTotalCount = clause.SetCount;
            return true;
        }
    }
}
