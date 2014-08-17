namespace Simple.OData.Client
{
    public interface IEdmModelParser
    {
        string[] SupportedProtocols { get; }
        EdmEntityType[] EntityTypes { get; }
        EdmComplexType[] ComplexTypes { get; }
        EdmEntityContainer[] EntityContainers { get; }
    }
}