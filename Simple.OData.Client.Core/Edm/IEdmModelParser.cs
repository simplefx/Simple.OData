namespace Simple.OData.Client
{
    public interface IEdmModelParser
    {
        string[] SupportedProtocols { get; }
        EdmEntityType[] EntityTypes { get; }
        EdmComplexType[] ComplexTypes { get; }
        EdmEnumType[] EnumTypes { get; }
        EdmAssociation[] Associations { get; }
        EdmEntityContainer[] EntityContainers { get; }
    }
}