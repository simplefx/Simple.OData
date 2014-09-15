using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    abstract class MetadataBase : IMetadata
    {
        public abstract IEnumerable<string> GetEntitySetNames();
        public abstract string GetEntitySetExactName(string entitySetName);
        public abstract string GetEntitySetTypeName(string entitySetName);
        public abstract string GetEntitySetTypeNamespace(string entitySetName);
        public abstract string GetDerivedEntityTypeExactName(string entitySetName, string entityTypeName);
        public abstract bool EntitySetTypeRequiresOptimisticConcurrencyCheck(string entitySetName);
        public abstract string GetEntityTypeExactName(string entityTypeName);
        public abstract IEnumerable<string> GetStructuralPropertyNames(string entitySetName);
        public abstract bool HasStructuralProperty(string entitySetName, string propertyName);
        public abstract string GetStructuralPropertyExactName(string entitySetName, string propertyName);
        public abstract IEnumerable<string> GetDeclaredKeyPropertyNames(string entitySetName);
        public abstract bool HasNavigationProperty(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyExactName(string entitySetName, string propertyName);
        public abstract string GetNavigationPropertyPartnerName(string entitySetName, string propertyName);
        public abstract bool IsNavigationPropertyMultiple(string entitySetName, string propertyName);
        public abstract string GetFunctionExactName(string functionName);

        public EntityCollection GetEntityCollection(string entitySetName)
        {
            return new EntityCollection(GetEntitySetExactName(entitySetName));
        }

        public EntityCollection GetBaseEntityCollection(string entitySetPath)
        {
            return this.GetEntityCollection(entitySetPath.Split('/').First());
        }

        public EntityCollection GetConcreteEntityCollection(string entitySetPath)
        {
            var items = entitySetPath.Split('/');
            if (items.Count() > 1)
            {
                var baseEntitySet = this.GetEntityCollection(items[0]);
                var entitySet = string.IsNullOrEmpty(items[1])
                    ? baseEntitySet
                    : GetDerivedEntityCollection(baseEntitySet, items[1]);
                return entitySet;
            }
            else
            {
                return this.GetEntityCollection(entitySetPath);
            }
        }

        public EntityCollection GetDerivedEntityCollection(EntityCollection baseEntityCollection, string entityTypeName)
        {
            var actualName = GetDerivedEntityTypeExactName(baseEntityCollection.ActualName, entityTypeName);
            return new EntityCollection(actualName, baseEntityCollection);
        }

        public EntryDetails ParseEntryDetails(string collectionName, IDictionary<string, object> entryData, string contentId = null)
        {
            var entryDetails = new EntryDetails();

            foreach (var item in entryData)
            {
                if (this.HasStructuralProperty(collectionName, item.Key))
                {
                    entryDetails.AddProperty(item.Key, item.Value);
                }
                else if (this.HasNavigationProperty(collectionName, item.Key))
                {
                    if (this.IsNavigationPropertyMultiple(collectionName, item.Key))
                    {
                        var collection = item.Value as IEnumerable<object>;
                        if (collection != null)
                        {
                            foreach (var element in collection)
                            {
                                entryDetails.AddLink(item.Key, element, contentId);
                            }
                        }
                    }
                    else
                    {
                        entryDetails.AddLink(item.Key, item.Value, contentId);
                    }
                }
                else
                {
                    throw new UnresolvableObjectException(item.Key, String.Format("No property or association found for {0}.", item.Key));
                }
            }

            return entryDetails;
        }
    }
}