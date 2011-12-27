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
        private readonly List<SimpleReference> _columns;
        private readonly List<SimpleOrderByItem> _order;
        private Stack<SimpleQueryClauseBase> _processedClauses;
        private readonly Func<string, Table> _findTable;

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

        public CommandBuilder(Func<string, Table> findTable)
        {
            _findTable = findTable;
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

        public string BuildCommand(string tableName, string filter)
        {
            var table = _findTable(tableName);
            var command = table.ActualName + "?$filter=" + HttpUtility.UrlEncode(filter);
            return command;
        }

        public string BuildCommand(SimpleQuery query)
        {
            Build(query);

            var table = _findTable(query.TableName);
            var command = table.ActualName;
            string appendSymbol = "?";
            command += FormatFilter(table, ref appendSymbol);
            command += FormatSkip(table, ref appendSymbol);
            command += FormatTake(table, ref appendSymbol);
            command += FormatOrder(table, ref appendSymbol);
            command += FormatSelect(table, ref appendSymbol);

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

        private string FormatFilter(Table table, ref string appendSymbol)
        {
            string text = string.Empty;
            if (this.Criteria != null)
            {
                text += appendSymbol;
                text += "$filter=" + HttpUtility.UrlEncode(
                    new ExpressionFormatter(_findTable).Format(this.Criteria));
                appendSymbol = "&";
            }
            return text;
        }

        private string FormatOrder(Table table, ref string appendSymbol)
        {
            string text = string.Empty;
            if (this.Order != null && this.Order.Count() > 0)
            {
                var items = this.Order.Select(x => FormatOrderByItem(table, x));
                text += appendSymbol;
                text += "$orderby=" + string.Join(",", items);
                appendSymbol = "&";
            }
            return text;
        }

        private string FormatSkip(Table table, ref string appendSymbol)
        {
            string text = string.Empty;
            if (this.SkipCount > 0)
            {
                text += appendSymbol;
                text += "$skip=" + this.SkipCount.ToString();
                appendSymbol = "&";
            }
            return text;
        }

        private string FormatTake(Table table, ref string appendSymbol)
        {
            string text = string.Empty;
            if (this.TakeCount > 0)
            {
                text += appendSymbol;
                text += "$top=" + this.TakeCount.ToString();
                appendSymbol = "&";
            }
            return text;
        }

        private string FormatSelect(Table table, ref string appendSymbol)
        {
            string text = string.Empty;
            if (!this.IsTotalCountQuery && this.Columns != null && this.Columns.Count() > 0)
            {
                var items = this.Columns.Select(x => FormatSelectItem(table, x));
                text += appendSymbol;
                text += "$select=" + string.Join(",", items);
                appendSymbol = "&";
            }
            return text;
        }
    }
}
