using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface IMetadata
    {
        string GetEntitySetExactName(string entitySetName);
        string GetEntitySetTypeName(string entitySetName);
        string GetEntitySetTypeNamespace(string entitySetName);
        bool EntitySetTypeRequiresOptimisticConcurrencyCheck(string entitySetName);

        string GetEntityTypeExactName(string entityTypeName);

        IEnumerable<string> GetStructuralPropertyNames(string entitySetName);
        bool HasStructuralProperty(string entitySetName, string propertyName);
        string GetStructuralPropertyExactName(string entitySetName, string propertyName);
        IEnumerable<string> GetDeclaredKeyPropertyNames(string entitySetName);

        bool HasNavigationProperty(string entitySetName, string propertyName);
        string GetNavigationPropertyExactName(string entitySetName, string propertyName);
        string GetNavigationPropertyPartnerName(string entitySetName, string propertyName);
        bool IsNavigationPropertyMultiple(string entitySetName, string propertyName);

        string GetFunctionExactName(string functionName);

        EntityCollection GetEntityCollection(string entitySetName);
        EntityCollection GetBaseEntityCollection(string entitySetPath);
        EntityCollection GetConcreteEntityCollection(string entitySetPath);
        EntityCollection GetDerivedEntityCollection(EntityCollection baseEntityCollection, string entityTypeName);

        EntryDetails ParseEntryDetails(string collectionName, IDictionary<string, object> entryData, string contentId = null);
    }
}