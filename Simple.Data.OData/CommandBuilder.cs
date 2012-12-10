using System;
using System.Linq;
using System.Collections.Generic;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    class CommandBuilder
    {
        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, QueryCommand, bool>> _processors;
        private readonly Func<string, Table> _findTable;
        private readonly Func<string, IList<string>> _getKeyNames;

        public CommandBuilder(Func<string, Table> findTable, Func<string, IList<string>> getKeyNames)
        {
            _findTable = findTable;
            _getKeyNames = getKeyNames;

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
            string tableName = ExtractPrimaryTableName(tablePath);
            var namedKeyValues = TryInterpretExpressionAsKeyLookup(tableName, criteria, _getKeyNames);
            return new QueryCommand()
                {
                    TablePath = tablePath,
                    Criteria = criteria,
                    NamedKeyValues = namedKeyValues == null ? null : namedKeyValues.ToDictionary(),
                    FilterExpression = namedKeyValues == null ? new ExpressionConverter().ConvertExpression(criteria) : null,
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

            var namedKeyValues = TryInterpretExpressionAsKeyLookup(query.TableName, cmd.Criteria, _getKeyNames);
            if (namedKeyValues != null)
            {
                cmd.NamedKeyValues = namedKeyValues.ToDictionary();
                cmd.FilterExpression = null;
                cmd.TakeCount = null;
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

            cmd.FilterExpression = new ExpressionConverter().ConvertExpression(cmd.Criteria);

            return true;
        }

        private bool TryApplyWithCountClause(WithCountClause clause, QueryCommand cmd)
        {
            cmd.SetTotalCount = clause.SetCount;
            return true;
        }

        private IDictionary<string, object> TryInterpretExpressionAsKeyLookup(string tablePath, SimpleExpression expression, Func<string, IEnumerable<string>> GetKeyNames)
        {
            var table = _findTable(ExtractPrimaryTableName(tablePath));
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (expression != null)
            {
                ExtractEqualityComparisons(expression, namedKeyValues);
            }
            return GetKeyNames(table.ActualName).All(namedKeyValues.ContainsKey) ? namedKeyValues : null;
        }

        private void ExtractEqualityComparisons(SimpleExpression expression, IDictionary<string, object> columnEqualityComparisons)
        {
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                    ExtractEqualityComparisons(expression.LeftOperand as SimpleExpression, columnEqualityComparisons);
                    ExtractEqualityComparisons(expression.RightOperand as SimpleExpression, columnEqualityComparisons);
                    break;

                case SimpleExpressionType.Equal:
                    if (expression.LeftOperand.GetType() == typeof(ObjectReference))
                    {
                        var key = expression.LeftOperand.ToString().Split('.').Last();
                        if (!columnEqualityComparisons.ContainsKey(key))
                            columnEqualityComparisons.Add(key, expression.RightOperand);
                    }
                    break;

                default:
                    break;
            }
        }

        internal static string[] ExtractTableNames(string tablePath)
        {
            return tablePath.Split('.');
        }

        internal static string ExtractPrimaryTableName(string tablePath)
        {
            return ExtractTableNames(tablePath).First();
        }
    }
}
