namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        private Table _table;

        public ODataClientWithCommand Client { get; set; }
        public Table Table
        {
            get { return _table ?? (IsSet ? this.Client.Schema.FindTable(this.CollectionName) : null); }
            set { _table = value; }
        }
        public string CollectionName { get; set; }

        public bool IsSet
        {
            get { return this.Client != null && (this._table != null || !string.IsNullOrEmpty(this.CollectionName)); }
        }
    }
}
