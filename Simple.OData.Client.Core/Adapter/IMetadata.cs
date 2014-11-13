using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface IMetadata
    {
        EntityCollection GetEntityCollection(string collectionPath);
        EntityCollection GetDerivedEntityCollection(EntityCollection baseCollection, string entityTypeName);

        string GetEntityCollectionExactName(string collectionName);
        string GetEntityCollectionTypeName(string collectionName);
        string GetEntityCollectionTypeNamespace(string collectionName);
        string GetEntityCollectionQualifiedTypeName(string collectionName);
        bool EntityCollectionRequiresOptimisticConcurrencyCheck(string collectionName);

        string GetEntityTypeExactName(string entityTypeName);

        IEnumerable<string> GetStructuralPropertyNames(string collectionName);
        bool HasStructuralProperty(string collectionName, string propertyName);
        string GetStructuralPropertyExactName(string collectionName, string propertyName);
        IEnumerable<string> GetDeclaredKeyPropertyNames(string collectionName);

        bool HasNavigationProperty(string collectionName, string propertyName);
        string GetNavigationPropertyExactName(string collectionName, string propertyName);
        string GetNavigationPropertyPartnerName(string collectionName, string propertyName);
        bool IsNavigationPropertyCollection(string collectionName, string propertyName);

        string GetFunctionExactName(string functionName);
        string GetActionExactName(string actionName);

        EntryDetails ParseEntryDetails(string collectionName, IDictionary<string, object> entryData, string contentId = null);
    }
}