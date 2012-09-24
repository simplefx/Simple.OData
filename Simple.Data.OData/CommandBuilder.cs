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
        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, bool>> _processors;
        private readonly List<string> _expand;
        private readonly List<SimpleReference> _columns;
        private readonly List<SimpleOrderByItem> _order;
        private readonly Func<string, Table> _findTable;
        private readonly Func<string, IList<string>> _getKeyNames;
        private readonly ExpressionFormatter _expressionFormatter;
        private Stack<SimpleQueryClauseBase> _processedClauses;
        public IEnumerable<SimpleQueryClauseBase> UnprocessedClauses { get; private set; }

        public IEnumerable<SimpleReference> Columns
        {
            get { return _columns; }
        }

        public IEnumerable<string> Expand { get { return _expand; } }

        public SimpleExpression Criteria { get; private set; }

        public IEnumerable<SimpleOrderByItem> Order
        {
            get { return _order; }
        }

        public int? SkipCount { get; private set; }

        public int? TakeCount { get; private set; }

        public Action<int> SetTotalCount { get; private set; }

        public bool IsScalarResult { get; private set; }

        public CommandBuilder(Func<string, Table> findTable, Func<string, IList<string>> getKeyNames)
        {
            _findTable = findTable;
            _getKeyNames = getKeyNames;
            _expressionFormatter = new ExpressionFormatter(_findTable);
            _columns = new List<SimpleReference>();
            _order = new List<SimpleOrderByItem>();
            _expand = new List<string>();
            _processedClauses = new Stack<SimpleQueryClauseBase>();

            _processors = new Dictionary<Type, Func<SimpleQueryClauseBase, bool>>
            {
                { typeof(OrderByClause), c => TryApplyOrderByClause((OrderByClause)c) },
                { typeof(SelectClause), c => TryApplySelectClause((SelectClause)c) },
                { typeof(SkipClause), c => TryApplySkipClause((SkipClause)c) },
                { typeof(TakeClause), c => TryApplyTakeClause((TakeClause)c) },
                { typeof(WhereClause), c => TryApplyWhereClause((WhereClause)c) },
                { typeof(WithClause), c => TryApplyWithClause((WithClause)c) },
                { typeof(WithCountClause), c => TryApplyWithCountClause((WithCountClause)c) },
            };
        }

        public string BuildCommand(string tablePath, SimpleExpression criteria)
        {
            string commandText;
            string tableName = ExtractPrimaryTableName(tablePath);
            var keyLookup = TryFormatCriteriaAsKeyLookup(tableName, _expressionFormatter.Format(criteria), _getKeyNames);
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
            return commandText;
        }

        public string BuildCommand(SimpleQuery query)
        {
            Build(query);

            Table table;
            var formattedKeyValues = TryFormatCriteriaAsKeyLookup(query.TableName, _expressionFormatter.Format(this.Criteria), _getKeyNames);
            bool isKeyLookup = !string.IsNullOrEmpty(formattedKeyValues);
            string clause = FormatTableClause(query.TableName, formattedKeyValues, out table);
            var commandText = clause;

            clause = FormatSpecialClause(table);
            if (!string.IsNullOrEmpty(clause))
            {
                commandText += "/" + clause;
                this.IsScalarResult = true;
            }

            var clauses = new List<string>();
            if (!isKeyLookup)
            {
                clause = FormatWhereClause(table);
                if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            }
            clause = FormatWithClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSkipClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            if (!isKeyLookup)
            {
                clause = FormatTakeClause(table);
                if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            }
            clause = FormatOrderClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSelectClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatCountClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            if (clauses.Count > 0)
            {
                commandText += "?" + string.Join("&", clauses);
            }

            return commandText;
        }

        public string BuildCommand(string tablePath, object[] keyValues)
        {
            return FormatTableKeyLookup(tablePath, FormatKeyValues(keyValues));
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

            this.UnprocessedClauses = unprocessedClauses;
        }

        private bool TryApplyWithClause(WithClause clause)
        {
            _expand.Add(clause.ObjectReference.GetName());
            return true;
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

            Criteria = Criteria == null
                ? clause.Criteria
                : new SimpleExpression(Criteria, clause.Criteria, SimpleExpressionType.And);
            return true;
        }

        private bool TryApplyWithCountClause(WithCountClause clause)
        {
            SetTotalCount = clause.SetCount;
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

        private string FormatSpecialClause(Table table)
        {
            if (this.Columns != null && this.Columns.Count() == 1 && this.Columns.First() is SpecialReference)
            {
                var specialColumn = _columns[0];
                _columns.Clear();
                return FormatSpecialReference((SpecialReference)specialColumn);
            }
            return null;
        }

        private string FormatWhereClause(Table table)
        {
            if (this.Criteria != null)
            {
                return FormatFilter(_expressionFormatter.Format(this.Criteria));
            }
            return null;
        }

        private string FormatWithClause(Table table)
        {
            if (this.Expand.Any())
            {
                var expansion = string.Join(",", this.Expand);
                return "$expand=" + expansion;
            }
            return null;
        }

        private string FormatOrderClause(Table table)
        {
            if (this.Order.Any())
            {
                var items = this.Order.Select(x => FormatOrderByItem(table, x));
                return "$orderby=" + string.Join(",", items);
            }
            return null;
        }

        private string FormatSkipClause(Table table)
        {
            if (this.SkipCount > 0)
            {
                return "$skip=" + this.SkipCount.ToString();
            }
            return null;
        }

        private string FormatTakeClause(Table table)
        {
            if (this.TakeCount > 0)
            {
                return "$top=" + this.TakeCount.ToString();
            }
            return null;
        }

        private string FormatSelectClause(Table table)
        {
            if (this.Columns != null && this.Columns.Count() > 0)
            {
                var items = this.Columns.Select(x => FormatSelectItem(table, x));
                return "$select=" + string.Join(",", items);
            }
            return null;
        }

        private string FormatCountClause(Table table)
        {
            if (this.SetTotalCount != null)
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

        private string TryFormatCriteriaAsKeyLookup(string tablePath, string formattedCriteria, Func<string, IEnumerable<string>> GetKeyNames)
        {
            var table = _findTable(ExtractPrimaryTableName(tablePath));

            IList<string> keyValues = new List<string>();
            var keyNames = GetKeyNames(table.ActualName);
            var filterItems = formattedCriteria.Split(',');
            int processedKeys = 0;
            if (keyNames.Count() == filterItems.Count())
            {
                foreach (var keyName in keyNames)
                {
                    foreach (var filterItem in filterItems)
                    {
                        int index = filterItem.IndexOf(" eq ");
                        if (index > 0 && filterItem.Substring(0, index) == keyName)
                        {
                            var keyValue = filterItem.Substring(index + 4);
                            keyValues.Add(keyValue);
                            ++processedKeys;
                        }
                    }
                }
            }
                
            return processedKeys == keyNames.Count() ? FormatKeyValues(keyValues) : string.Empty;
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
