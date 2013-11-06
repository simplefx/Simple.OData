using System;
using System.Collections.Generic;
using Simple.OData.Client;

namespace Simple.Data.OData
{
    class QueryCommand
    {
        private readonly List<string> _expand;
        private readonly List<SimpleReference> _columns;
        private readonly List<SimpleOrderByItem> _order;

        public SimpleExpression Criteria { get; set; }
        public ODataExpression FilterExpression { get; set; }
        public List<object> KeyValues { get; set; }
        public Dictionary<string, object> NamedKeyValues { get; set; }
        public List<SimpleReference> Columns { get { return _columns; } }
        public List<string> Expand { get { return _expand; } }
        public List<SimpleOrderByItem> Order { get { return _order; } }
        public int? SkipCount { get; set; }
        public int? TakeCount { get; set; }
        public Action<int> SetTotalCount { get; set; }
        public bool IsScalarResult { get; set; }

        public string TablePath { get; set; }
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
}