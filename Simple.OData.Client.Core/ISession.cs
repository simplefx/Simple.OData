namespace Simple.OData.Client
{
    internal interface ISession
    {
        ODataProvider Provider { get; }
        IPluralizer Pluralizer { get; }
    }
}