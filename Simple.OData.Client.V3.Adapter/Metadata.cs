using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;

#pragma warning disable 1591

namespace Simple.OData.Client.V3.Adapter
{
    public class Metadata : MetadataBase
    {
        private readonly ISession _session;
        private readonly IEdmModel _model;

        public Metadata(ISession session, IEdmModel model)
        {
            _session = session;
            _model = model;
        }

        public override ISession Session { get { return _session; } }

        public override string GetEntityCollectionExactName(string collectionName)
        {
            IEdmEntitySet entitySet;
            IEdmEntityType entityType;
            if (TryGetEntitySet(collectionName, out entitySet))
            {
                return entitySet.Name;
            }
            else if (TryGetEntityType(collectionName, out entityType))
            {
                return entityType.Name;
            }

            throw new UnresolvableObjectException(collectionName, string.Format("Entity collection {0} not found", collectionName));
        }

        public override string GetEntityCollectionTypeName(string collectionName)
        {
            return GetEntityType(collectionName).Name;
        }

        public override string GetEntityCollectionTypeNamespace(string collectionName)
        {
            return GetEntityType(collectionName).Namespace;
        }

        public override bool EntityCollectionRequiresOptimisticConcurrencyCheck(string collectionName)
        {
            return GetEntityType(collectionName).StructuralProperties()
                .Any(x => x.ConcurrencyMode == EdmConcurrencyMode.Fixed);
        }

        public override string GetDerivedEntityTypeExactName(string collectionName, string entityTypeName)
        {
            IEdmEntitySet entitySet;
            IEdmEntityType entityType;
            if (TryGetEntitySet(collectionName, out entitySet))
            {
                entityType = (_model.FindDirectlyDerivedTypes(entitySet.ElementType)
                    .BestMatch(x => (x as IEdmEntityType).Name, entityTypeName, _session.Pluralizer) as IEdmEntityType);
                if (entityType != null)
                    return entityType.Name;
            }
            else if (TryGetEntityType(entityTypeName, out entityType))
            {
                return entityType.Name;
            }

            throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));
        }

        public override string GetEntityTypeExactName(string collectionName)
        {
            var entityType = GetEntityTypes().BestMatch(x => x.Name, collectionName, _session.Pluralizer);
            if (entityType != null)
                return entityType.Name;

            throw new UnresolvableObjectException(collectionName, string.Format("Entity type {0} not found", collectionName));
        }

        public override string GetLinkedCollectionName(string typeName)
        {
            var linkSet = _model.EntityContainers()
                .SelectMany(x => x.EntitySets())
                .BestMatch(x => x.ElementType.Name, typeName, _session.Pluralizer);
            if (linkSet != null)
                return linkSet.Name;

            throw new UnresolvableObjectException(typeName, string.Format("Linked collection for type {0} not found", typeName));
        }

        public override bool IsOpenType(string collectionName)
        {
            return GetEntityType(collectionName).IsOpen;
        }

        public override bool IsTypeWithId(string collectionName)
        {
            return GetEntityType(collectionName).DeclaredKey != null;
        }

        public override IEnumerable<string> GetStructuralPropertyNames(string collectionName)
        {
            return GetEntityType(collectionName).StructuralProperties().Select(x => x.Name);
        }

        public override bool HasStructuralProperty(string collectionName, string propertyName)
        {
            return GetEntityType(collectionName).StructuralProperties().Any(x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));
        }

        public override string GetStructuralPropertyExactName(string collectionName, string propertyName)
        {
            return GetStructuralProperty(collectionName, propertyName).Name;
        }

        public override bool HasNavigationProperty(string collectionName, string propertyName)
        {
            return GetEntityType(collectionName).NavigationProperties().Any(x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));
        }

        public override string GetNavigationPropertyExactName(string collectionName, string propertyName)
        {
            return GetNavigationProperty(collectionName, propertyName).Name;
        }

        public override string GetNavigationPropertyPartnerName(string collectionName, string propertyName)
        {
            var navigationProperty = GetNavigationProperty(collectionName, propertyName);
            IEdmEntityType entityType;
            if (!TryGetEntityType(navigationProperty.Type, out entityType))
                throw new UnresolvableObjectException(propertyName, string.Format("No association found for {0}.", propertyName));
            return entityType.Name;
        }

        public override bool IsNavigationPropertyCollection(string collectionName, string propertyName)
        {
            var property = GetNavigationProperty(collectionName, propertyName);
            return property.Type.Definition.TypeKind == EdmTypeKind.Collection;
        }

        public override IEnumerable<string> GetDeclaredKeyPropertyNames(string collectionName)
        {
            var entityType = GetEntityType(collectionName);
            while (entityType.DeclaredKey == null && entityType.BaseEntityType() != null)
            {
                entityType = entityType.BaseEntityType();
            }

            if (entityType.DeclaredKey == null)
                return new string[] { };

            return entityType.DeclaredKey.Select(x => x.Name);
        }

        public override string GetFunctionFullName(string functionName)
        {
            var function = GetFunction(functionName);
            return function.Name;
        }

        public override EntityCollection GetFunctionReturnCollection(string functionName)
        {
            var function = GetFunction(functionName);

            if (function.ReturnType == null)
                return null;

            IEdmEntityType entityType;
            return !TryGetEntityType(function.ReturnType, out entityType) ? null : new EntityCollection(entityType.Name);
        }

        public override string GetActionFullName(string actionName)
        {
            return GetFunctionFullName(actionName);
        }

        public override EntityCollection GetActionReturnCollection(string actionName)
        {
            return GetFunctionReturnCollection(actionName);
        }

        private IEnumerable<IEdmEntitySet> GetEntitySets()
        {
            return _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets());
        }

        private IEdmEntitySet GetEntitySet(string entitySetName)
        {
            IEdmEntitySet entitySet;
            if (TryGetEntitySet(entitySetName, out entitySet))
                return entitySet;

            throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));
        }

        private bool TryGetEntitySet(string entitySetName, out IEdmEntitySet entitySet)
        {
            if (entitySetName.Contains("/"))
                entitySetName = entitySetName.Split('/').First();

            entitySet = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .BestMatch(x => x.Name, entitySetName, _session.Pluralizer);

            return entitySet != null;
        }

        private IEnumerable<IEdmEntityType> GetEntityTypes()
        {
            return _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition && (x as IEdmType).TypeKind == EdmTypeKind.Entity)
                .Select(x => x as IEdmEntityType);
        }

        private IEdmEntityType GetEntityType(string collectionName)
        {
            IEdmEntityType entityType;
            if (TryGetEntityType(collectionName, out entityType))
                return entityType;

            throw new UnresolvableObjectException(collectionName, string.Format("Entity type {0} not found", collectionName));
        }

        private bool TryGetEntityType(string collectionName, out IEdmEntityType entityType)
        {
            entityType = null;
            if (collectionName.Contains("/"))
            {
                var items = collectionName.Split('/');
                collectionName = items.First();
                var derivedTypeName = items.Last();

                if (derivedTypeName.Contains("."))
                {
                    var derivedType = GetEntityTypes().SingleOrDefault(x => x.FullName() == derivedTypeName);
                    if (derivedType != null)
                    {
                        entityType = derivedType;
                        return true;
                    }
                }
                else
                {
                    var entitySet = GetEntitySets()
                        .BestMatch(x => x.Name, collectionName, _session.Pluralizer);
                    if (entitySet != null)
                    {
                        var derivedType = GetEntityTypes().BestMatch(x => x.Name, derivedTypeName, _session.Pluralizer);
                        if (derivedType != null)
                        {
                            if (_model.FindDirectlyDerivedTypes(entitySet.ElementType).Contains(derivedType))
                            {
                                entityType = derivedType;
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                var entitySet = GetEntitySets()
                    .BestMatch(x => x.Name, collectionName, _session.Pluralizer);
                if (entitySet != null)
                {
                    entityType = entitySet.ElementType;
                    return true;
                }

                var derivedType = GetEntityTypes().BestMatch(x => x.Name, collectionName, _session.Pluralizer);
                if (derivedType != null)
                {
                    var baseType = GetEntityTypes()
                        .SingleOrDefault(x => _model.FindDirectlyDerivedTypes(x).Contains(derivedType));
                    if (baseType != null && GetEntitySets().SingleOrDefault(x => x.ElementType == baseType) != null)
                    {
                        entityType = derivedType;
                        return true;
                    }
                    // Check if we can return it anyway
                    entityType = derivedType;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetEntityType(IEdmTypeReference typeReference, out IEdmEntityType entityType)
        {
            entityType = typeReference.Definition.TypeKind == EdmTypeKind.Collection
                ? (typeReference.Definition as IEdmCollectionType).ElementType.Definition as IEdmEntityType
                : typeReference.Definition.TypeKind == EdmTypeKind.Entity
                ? typeReference.Definition as IEdmEntityType
                : null;
            return entityType != null;
        }

        private IEdmStructuralProperty GetStructuralProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).StructuralProperties().BestMatch(
                x => x.Name, propertyName, _session.Pluralizer);

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Structural property {0} not found", propertyName));

            return property;
        }

        private IEdmNavigationProperty GetNavigationProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).NavigationProperties().BestMatch(
                x => x.Name, propertyName, _session.Pluralizer);

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Navigation property {0} not found", propertyName));

            return property;
        }

        private IEdmFunctionImport GetFunction(string functionName)
        {
            var function = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).FunctionImports())
                .BestMatch(x => x.Name, functionName, _session.Pluralizer);

            if (function == null)
                throw new UnresolvableObjectException(functionName, string.Format("Function {0} not found", functionName));

            return function;
        }
    }
}