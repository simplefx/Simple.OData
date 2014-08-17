namespace Simple.OData.Client
{
    abstract class ProviderMetadata
    {
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract bool HasNavigationProperty(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyActualName(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyPartnerName(string entitySetName, string propertyName);
        public abstract bool IsNavigationPropertyMultiple(string entitySetName, string propertyName);
        public abstract string GetFunctionActualName(string functionName);
    }
}