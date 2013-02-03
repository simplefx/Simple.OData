using System.Linq;

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

                return this.Client.Schema.FindConcreteTable(this.Collection);
            }

            set 
            { 
                _table = value; 
            }
        }
        public string Collection { get; set; }

        public bool IsSet
        {
            get { return this.Client != null && (this._table != null || !string.IsNullOrEmpty(this.Collection)); }
        }
    }
}
