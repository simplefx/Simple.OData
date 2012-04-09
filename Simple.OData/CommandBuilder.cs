using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Simple.Data;
using Simple.OData.Schema;

namespace Simple.OData
{
    public class CommandBuilder
    {
        private readonly Dictionary<Type, Func<SimpleQueryClauseBase, bool>> _processors;
        private readonly List<string> _expand;
        private readonly List<SimpleReference> _columns;
        private readonly List<SimpleOrderByItem> _order;
        private Stack<SimpleQueryClauseBase> _processedClauses;
        private readonly Func<string, Table> _findTable;

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

        public IEnumerable<SimpleQueryClauseBase> UnprocessedClauses { get; private set; }

        public bool IsScalarResult { get; private set; }

        public CommandBuilder(Func<string, Table> findTable)
        {
            _findTable = findTable;
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

        public string BuildCommand(string tableName, string filter)
        {
            var table = _findTable(tableName);
            var command = table.ActualName;
            if (!string.IsNullOrEmpty(filter))
                command += "?$filter=" + HttpUtility.UrlEncode(filter);
            return command;
        }

        public string BuildCommand(SimpleQuery query)
        {
            Build(query);

            var table = _findTable(query.TableName);
            var command = table.ActualName;

            string clause = FormatSpecialClause(table);
            if (!string.IsNullOrEmpty(clause))
            {
                command += "/" + clause;
                this.IsScalarResult = true;
            }

            var clauses = new List<string>();
            clause = FormatWhereClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatWithClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSkipClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatTakeClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatOrderClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatSelectClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            clause = FormatCountClause(table);
            if (!string.IsNullOrEmpty(clause)) clauses.Add(clause);
            if (clauses.Count > 0)
            {
                command += "?" + string.Join("&", clauses);
            }

            return command;
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
                return "$filter=" + HttpUtility.UrlEncode(
                    new ExpressionFormatter(_findTable).Format(this.Criteria));
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
    }
}
