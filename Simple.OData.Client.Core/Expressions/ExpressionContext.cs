namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        private EntityCollection _entityCollection;

        public Session Session { get; set; }
        public EntityCollection EntityCollection
        {
            get
            {
                if (!IsSet)
                    return null;
                if (_entityCollection != null)
                    return _entityCollection;

                return this.Session.Provider.GetMetadata().GetConcreteEntityCollection(this.Collection);
            }

            set 
            { 
                _entityCollection = value; 
            }
        }
        public string Collection { get; set; }

        public bool IsSet
        {
            get { return this.Session != null && (this._entityCollection != null || !string.IsNullOrEmpty(this.Collection)); }
        }
    }
}
