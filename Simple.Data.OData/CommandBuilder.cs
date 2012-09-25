using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Simple.Data;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData
{
    public class CommandBuilder
    {
        public class QueryCommand
        {
            private readonly List<string> _expand;
            private readonly List<SimpleReference> _columns;
            private readonly List<SimpleOrderByItem> _order;

            public SimpleExpression Criteria { get; set; }
            public List<SimpleReference> Columns { get { return _columns; } }
            public List<string> Expand { get { return _expand; } }
            public List<SimpleOrderByItem> Order { get { return _order; } }
            public int? SkipCount { get; set; }
            public int? TakeCount { get; set; }
            public Action<int> SetTotalCount { get; set; }
            public bool IsScalarResult { get; set; }

            public string CommandText { get; set; }
            public Stack<SimpleQueryClauseBase> ProcessedClauses { get; private set; }
            public IEnumerable<SimpleQueryClauseBase> UnprocessedClauses { get; set; }

            public QueryCommand()
            {
                this.ProcessedClauses = new Stack<SimpleQueryClauseBase>();
                _columns = new List<SimpleReference>();
                _order = new List<SimpleOrderByItem>();
                _expand = new List<string>();
            }
        }

        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, QueryCommand, bool>> _processors;
        private readonly Func<string, Table> _findTable;
        private readonly Func<string, IList<string>> _getKeyNames;
        private readonly ExpressionFormatter _expressionFormatter;

        public CommandBuilder(Func<string, Table> findTable, Func<string, IList<string>> getKeyNames)
        {
            _findTable = findTable;
            _getKeyNames = getKeyNames;
            _expressionFormatter = new ExpressionFormatter(_findTable);

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
            string commandText;
            string tableName = ExtractPrimaryTableName(tablePath);
            var keyLookup = TryFormatExpressionAsKeyLookup(tableName, criteria, _getKeyNames);
            if (!string.IsNullOrEmpty(keyLookup))
            {
                commandText = FormatTableKeyLookup(tablePath, keyLookup);
            }
            else
            {
                commandText = _findTable(tableName).ActualName;
                string filter = _expressionFormatter.Format(criteria);
                if (!string.IsNullOrEmpty(filter))
                    commandText += "?" + FormatFilter(filter);
            }
            return new QueryCommand() { Criteria = criteria, CommandText = commandText};
        }

        public QueryCommand BuildCommand(SimpleQuery query)
        {
            var cmd = ParseQuery(query);

            Table table;
            var formattedKeyValues = TryFormatExpressionAsKeyLookup(query.TableName, cmd.Criteria, _getKeyNames);
            bool isKeyLookup = !string.IsNullOrEmpty(formattedKeyValues);
            string clause = FormatTableClause(query.TableName, formattedKeyValues, out table);
            var commandText = clause;

            clause = FormatSpecialClause(table, cmd);
            if (!string.IsNullOrEmpty(clause))
            {
                commandText += "/" + clause;
                cmd.IsScalarResult = true;
            }

            var clauses = new List<string>();
            if (!isKeyLookup)
            {
                clause = FormatWhereClause(table, cmd);
                if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            }
            clause = FormatWithClause(table, cmd);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSkipClause(table, cmd);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            if (!isKeyLookup)
            {
                clause = FormatTakeClause(table, cmd);
                if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            }
            clause = FormatOrderClause(table, cmd);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSelectClause(table, cmd);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatCountClause(table, cmd);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            if (clauses.Count > 0)
            {
                commandText += "?" + string.Join("&", clauses);
            }
            cmd.CommandText = commandText;

            return cmd;
        }

        public QueryCommand BuildCommand(string tablePath, object[] keyValues)
        {
            var commandText = FormatTableKeyLookup(tablePath, FormatKeyValues(keyValues));
            return new QueryCommand() { CommandText = commandText };
        }

        private QueryCommand ParseQuery(SimpleQuery query)
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

            cmd.UnprocessedClauses = unprocessedClauses;
            return cmd;
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
            return true;
        }

        private bool TryApplyWithCountClause(WithCountClause clause, QueryCommand cmd)
        {
            cmd.SetTotalCount = clause.SetCount;
            return true;
        }

        private string FormatTableKeyLookup(string tablePath, string formattedKeyValues)
        {
            Table table;
            return FormatTableClause(tablePath, formattedKeyValues, out table);
        }

        private string FormatTableClause(string tablePath, string formattedKeyValues, out Table table)
        {
            string clause = string.Empty;
            var tableNames = ExtractTableNames(tablePath);
            table = null;
            if (tableNames.Count() > 1)
            {
                Table parentTable = null;
                foreach (var tableName in tableNames)
                {
                    if (parentTable == null)
                    {
                        var childTable = _findTable(tableName);
                        table = childTable;
                        clause += childTable.ActualName;
                        if (!string.IsNullOrEmpty(formattedKeyValues))
                            clause += formattedKeyValues;
                        parentTable = childTable;
                    }
                    else
                    {
                        var association = parentTable.FindAssociation(tableName);
                        parentTable = _findTable(association.ReferenceTableName);
                        clause += "/" + association.ActualName;
                    }
                }
            }
            else
            {
                table = _findTable(tablePath);
                clause = table.ActualName;
                if (!string.IsNullOrEmpty(formattedKeyValues))
                    clause += formattedKeyValues;
            }
            return clause;
        }

        private string FormatOrderByItem(Table table, SimpleOrderByItem item)
        {
            var col = table.FindColumn(item.Reference.GetName());
            var direction = item.Direction == OrderByDirection.Descending ? " desc" : string.Empty;
            return col.ActualName + direction;
        }

        private string FormatSelectItem(Table table, SimpleReference item)
        {
            return table.FindColumn(item.GetAliasOrName()).ActualName;
        }

        private string FormatSpecialReference(SpecialReference reference)
        {
            if (reference.GetType() == typeof(CountSpecialReference)) return "$count";
            throw new InvalidOperationException("SpecialReference type not recognised.");
        }

        private string FormatSpecialClause(Table table, QueryCommand cmd)
        {
            if (cmd.Columns != null && cmd.Columns.Count() == 1 && cmd.Columns.First() is SpecialReference)
            {
                var specialColumn = cmd.Columns.First();
                cmd.Columns.Clear();
                return FormatSpecialReference((SpecialReference)specialColumn);
            }
            return null;
        }

        private string FormatWhereClause(Table table, QueryCommand cmd)
        {
            if (cmd.Criteria != null)
            {
                return FormatFilter(_expressionFormatter.Format(cmd.Criteria));
            }
            return null;
        }

        private string FormatWithClause(Table table, QueryCommand cmd)
        {
            if (cmd.Expand.Any())
            {
                var expansion = string.Join(",", cmd.Expand);
                return "$expand=" + expansion;
            }
            return null;
        }

        private string FormatOrderClause(Table table, QueryCommand cmd)
        {
            if (cmd.Order.Any())
            {
                var items = cmd.Order.Select(x => FormatOrderByItem(table, x));
                return "$orderby=" + string.Join(",", items);
            }
            return null;
        }

        private string FormatSkipClause(Table table, QueryCommand cmd)
        {
            if (cmd.SkipCount > 0)
            {
                return "$skip=" + cmd.SkipCount.ToString();
            }
            return null;
        }

        private string FormatTakeClause(Table table, QueryCommand cmd)
        {
            if (cmd.TakeCount > 0)
            {
                return "$top=" + cmd.TakeCount.ToString();
            }
            return null;
        }

        private string FormatSelectClause(Table table, QueryCommand cmd)
        {
            if (cmd.Columns != null && cmd.Columns.Count() > 0)
            {
                var items = cmd.Columns.Select(x => FormatSelectItem(table, x));
                return "$select=" + string.Join(",", items);
            }
            return null;
        }

        private string FormatCountClause(Table table, QueryCommand cmd)
        {
            if (cmd.SetTotalCount != null)
            {
                return "$inlinecount=allpages";
            }
            return null;
        }

        private string FormatFilter(string filter)
        {
            return "$filter=" + HttpUtility.UrlEncode(filter);
        }

        private string FormatKeyValues(IEnumerable<object> keyValues)
        {
            return "(" + HttpUtility.UrlEncode(_expressionFormatter.Format(keyValues)) + ")";
        }

        private string FormatKeyValues(IEnumerable<string> keyValues)
        {
            return "(" + HttpUtility.UrlEncode(string.Join(",", keyValues)) + ")";
        }

        private string TryFormatExpressionAsKeyLookup(string tablePath, SimpleExpression expression, Func<string, IEnumerable<string>> GetKeyNames)
        {
            var table = _findTable(ExtractPrimaryTableName(tablePath));
            IDictionary<string, object> namedKeyValues = new Dictionary<string, object>();
            if (expression != null)
            {
                TryExtractKeyValues(expression, namedKeyValues);
            }
            if (namedKeyValues.Count == 0)
                return string.Empty;

            IList<string> keyValues = new List<string>();
            var keyNames = GetKeyNames(table.ActualName);
            int processedKeys = 0;
            if (keyNames.Count() == namedKeyValues.Count())
            {
                foreach (var keyName in keyNames)
                {
                    foreach (var namedKeyValue in namedKeyValues)
                    {
                        if (namedKeyValue.Key == keyName)
                        {
                            keyValues.Add(_expressionFormatter.FormatContentValue(namedKeyValue.Value));
                            ++processedKeys;
                        }
                    }
                }
            }
                
            return processedKeys == keyNames.Count() ? FormatKeyValues(keyValues) : string.Empty;
        }

        private void TryExtractKeyValues(SimpleExpression expression, IDictionary<string, object> namedKeyValues)
        {
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                    TryExtractKeyValues(expression.LeftOperand as SimpleExpression, namedKeyValues);
                    TryExtractKeyValues(expression.RightOperand as SimpleExpression, namedKeyValues);
                    break;

                case SimpleExpressionType.Equal:
                    var key = expression.LeftOperand.ToString().Split('.').Last();
                    if (!namedKeyValues.ContainsKey(key))
                        namedKeyValues.Add(key, expression.RightOperand);
                    break;

                default:
                    break;
            }
        }

        private string[] ExtractTableNames(string tablePath)
        {
            return tablePath.Split('.');
        }

        private string ExtractPrimaryTableName(string tablePath)
        {
            return tablePath.Split('.').First();
        }
    }
}
