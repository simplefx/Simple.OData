namespace Simple.OData.Client
{
    internal class ExpressionContext
    {
        public ISession Session { get; set; }
        public EntityCollection EntityCollection { get; set; }
        public bool IsQueryOption { get; set; }

        public ExpressionContext(ISession session, EntityCollection entityCollection = null)
        {
            this.Session = session;
            this.EntityCollection = entityCollection;
        }

        public ExpressionContext(ISession session, bool isQueryOption)
        {
            this.Session = session;
            this.IsQueryOption = isQueryOption;
        }
    }
}
