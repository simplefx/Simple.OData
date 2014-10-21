using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public abstract class MetadataBase : IMetadata
    {
        public abstract string GetEntityCollectionExactName(string collectionName);
        public abstract string GetEntityCollectionTypeName(string collectionName);
        public abstract string GetEntityCollectionTypeNamespace(string collectionName);
        public abstract string GetDerivedEntityTypeExactName(string collectionName, string entityTypeName);
        public abstract bool EntityCollectionTypeRequiresOptimisticConcurrencyCheck(string collectionName);
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

        public EntityCollection GetEntityCollection(string collectionName)
        {
            return new EntityCollection(GetEntityCollectionExactName(collectionName));
        }

        public EntityCollection GetBaseEntityCollection(string collectionPath)
        {
            var segments = collectionPath.Split('/');
            if (segments.Count() > 1)
            {
                if (segments.Last().Contains("."))
                {
                    return this.GetEntityCollection(ExtractCollectionName(segments[segments.Length - 2]));
                }
                else
                {
                    return this.GetEntityCollection(ExtractCollectionName(segments.Last()));
                }
            }
            else
            {
                return this.GetEntityCollection(ExtractCollectionName(collectionPath));
            }
        }

        public EntityCollection GetConcreteEntityCollection(string collectionPath)
        {
            var segments = collectionPath.Split('/');
            if (segments.Count() > 1)
            {
                if (segments.Last().Contains("."))
                {
                    var baseEntitySet = this.GetEntityCollection(ExtractCollectionName(segments[segments.Length-2]));
                    return GetDerivedEntityCollection(baseEntitySet, ExtractCollectionName(segments.Last()));
                }
                else
                {
                    return this.GetEntityCollection(ExtractCollectionName(segments.Last()));
                }
            }
            else
            {
                return this.GetEntityCollection(ExtractCollectionName(collectionPath));
            }
        }

        public EntityCollection GetDerivedEntityCollection(EntityCollection baseCollection, string entityTypeName)
        {
            var actualName = GetDerivedEntityTypeExactName(baseCollection.ActualName, entityTypeName);
            return new EntityCollection(actualName, baseCollection);
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
                        if (item.Value == null)
                        {
                            entryDetails.AddLink(item.Key, null, contentId);
                        }
                        else
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

        private string ExtractCollectionName(string text)
        {
            if (text.Contains("("))
                return text.Substring(0, text.IndexOf('('));
            else
                return text;
        }
    }
}