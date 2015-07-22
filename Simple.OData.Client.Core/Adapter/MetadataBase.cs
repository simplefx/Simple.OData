using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public abstract class MetadataBase : IMetadata
    {
        public abstract ISession Session { get; }

        public abstract string GetEntityCollectionExactName(string collectionName);
        public abstract string GetEntityCollectionTypeName(string collectionName);
        public abstract string GetEntityCollectionTypeNamespace(string collectionName);

        public abstract string GetDerivedEntityTypeExactName(string collectionName, string entityTypeName);
        public abstract bool EntityCollectionRequiresOptimisticConcurrencyCheck(string collectionName);
        public abstract string GetEntityTypeExactName(string collectionName);
        public abstract string GetLinkedCollectionName(string instanceTypeName, string typeName, out bool isSingleton);
        public abstract bool IsOpenType(string collectionName);
        public abstract bool IsTypeWithId(string collectionName);
        public abstract IEnumerable<string> GetStructuralPropertyNames(string collectionName);
        public abstract bool HasStructuralProperty(string collectionName, string propertyName);
        public abstract string GetStructuralPropertyExactName(string collectionName, string propertyName);
        public abstract IEnumerable<string> GetDeclaredKeyPropertyNames(string collectionName);
        public abstract bool HasNavigationProperty(string collectionName, string propertyName);
        public abstract string GetNavigationPropertyExactName(string collectionName, string propertyName);
        public abstract string GetNavigationPropertyPartnerName(string collectionName, string propertyName);
        public abstract bool IsNavigationPropertyCollection(string collectionName, string propertyName);
        public abstract string GetFunctionFullName(string functionName);
        public abstract EntityCollection GetFunctionReturnCollection(string functionName);
        public abstract string GetActionFullName(string actionName);
        public abstract EntityCollection GetActionReturnCollection(string actionName);

        public EntityCollection GetEntityCollection(string collectionPath)
        {
            var segments = collectionPath.Split('/');
            if (segments.Count() > 1)
            {
                if (segments.Last().Contains("."))
                {
                    var baseEntitySet = this.GetEntityCollection(Utils.ExtractCollectionName(segments[segments.Length - 2]));
                    return GetDerivedEntityCollection(baseEntitySet, Utils.ExtractCollectionName(segments.Last()));
                }
                else
                {
                    return new EntityCollection(GetEntityCollectionExactName(Utils.ExtractCollectionName(segments.Last())));
                }
            }
            else
            {
                return new EntityCollection(GetEntityCollectionExactName(Utils.ExtractCollectionName(collectionPath)));
            }
        }

        public EntityCollection GetDerivedEntityCollection(EntityCollection baseCollection, string entityTypeName)
        {
            var actualName = GetDerivedEntityTypeExactName(baseCollection.Name, entityTypeName);
            return new EntityCollection(actualName, baseCollection);
        }

        public string GetEntityCollectionQualifiedTypeName(string collectionName)
        {
            var entityTypeNamespace = GetEntityCollectionTypeNamespace(collectionName);
            var entityTypeName = GetEntityCollectionTypeName(collectionName);
            return string.Join(".", entityTypeNamespace, entityTypeName);
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
                    if (this.IsNavigationPropertyCollection(collectionName, item.Key))
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
                else if (this.IsOpenType(collectionName))
                {
                    entryDetails.AddProperty(item.Key, item.Value);
                }
                else if (!this.Session.Settings.IgnoreUnmappedProperties)
                {
                    throw new UnresolvableObjectException(item.Key, String.Format("No property or association found for [{0}].", item.Key));
                }
            }

            return entryDetails;
        }

        //private string ExtractCollectionName(string text)
        //{
        //    if (text.Contains("("))
        //        return text.Substring(0, text.IndexOf('('));
        //    else
        //        return text;
        //}
    }
}