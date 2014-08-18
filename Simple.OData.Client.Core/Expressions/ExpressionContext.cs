namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        private EntitySet _entitySet;

        public Schema Schema { get; set; }
        public EntitySet EntitySet
        {
            get
            {
                if (!IsSet)
                    return null;
                if (_entitySet != null)
                    return _entitySet;

                return this.Schema.FindConcreteEntitySet(this.Collection);
            }

            set 
            { 
                _entitySet = value; 
            }
        }
        public string Collection { get; set; }

        public bool IsSet
        {
            get { return this.Schema != null && (this._entitySet != null || !string.IsNullOrEmpty(this.Collection)); }
        }
    }
}
