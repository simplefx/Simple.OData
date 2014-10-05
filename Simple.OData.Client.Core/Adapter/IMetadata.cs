using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface IMetadata
    {
        EntityCollection GetEntityCollection(string collectionName);
        EntityCollection GetBaseEntityCollection(string collectionPath);
        EntityCollection GetConcreteEntityCollection(string collectionPath);
        EntityCollection GetDerivedEntityCollection(EntityCollection baseCollection, string entityTypeName);

        string GetEntityCollectionExactName(string collectionName);
        string GetEntityCollectionTypeName(string collectionName);
        string GetEntityCollectionTypeNamespace(string collectionName);
        bool EntityCollectionTypeRequiresOptimisticConcurrencyCheck(string collectionName);

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

        EntryDetails ParseEntryDetails(string collectionName, IDictionary<string, object> entryData, string contentId = null);
    }
}