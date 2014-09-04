namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        private EntitySet _entitySet;

        public Session Session { get; set; }
        public EntitySet EntitySet
        {
            get
            {
                if (!IsSet)
                    return null;
                if (_entitySet != null)
                    return _entitySet;

                return this.Session.MetadataCache.FindConcreteEntitySet(this.Collection);
            }

            set 
            { 
                _entitySet = value; 
            }
        }
        public string Collection { get; set; }

        public bool IsSet
        {
            get { return this.Session != null && (this._entitySet != null || !string.IsNullOrEmpty(this.Collection)); }
        }
    }
}
