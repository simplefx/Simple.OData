namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        private Table _table;

        public ODataClientWithCommand Client { get; set; }
        public Table Table
        {
            get
            {
                if (!IsSet)
                    return null;
                if (_table != null)
                    return _table;

                var table = this.Client.Schema.FindTable(this.CollectionName);
                return string.IsNullOrEmpty(this.DerivedCollectionName)
                    ? table
                    : table.FindDerivedTable(this.DerivedCollectionName);
            }

            set 
            { 
                _table = value; 
            }
        }
        public string CollectionName { get; set; }
        public string DerivedCollectionName { get; set; }

        public bool IsSet
        {
            get { return this.Client != null && (this._table != null || !string.IsNullOrEmpty(this.CollectionName)); }
        }
    }
}
