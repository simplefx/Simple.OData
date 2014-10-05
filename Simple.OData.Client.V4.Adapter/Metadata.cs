using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.V4.Adapter
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

        public override IEnumerable<string> GetEntityCollectionNames()
        {
            return GetEntitySets().Select(x => x.Name)
                .Union(GetSingletons().Select(x => x.Name));
        }

        public override string GetEntityCollectionExactName(string collectionName)
        {
            IEdmEntitySet entitySet;
            IEdmSingleton singleton;
            if (TryGetEntitySet(collectionName, out entitySet))
            {
                return entitySet.Name;
            }
            else if (TryGetSingleton(collectionName, out singleton))
            {
                return singleton.Name;
            }

            throw new UnresolvableObjectException(collectionName, string.Format("Entity set or singleton {0} not found", collectionName));
        }

        public override string GetEntityCollectionTypeName(string collectionName)
        {
            return GetEntityType(collectionName).Name;
        }

        public override string GetEntityCollectionTypeNamespace(string collectionName)
        {
            return GetEntityType(collectionName).Namespace;
        }

        public override bool EntityCollectionTypeRequiresOptimisticConcurrencyCheck(string collectionName)
        {
            return GetEntityType(collectionName).StructuralProperties()
                .Any(x => x.ConcurrencyMode == EdmConcurrencyMode.Fixed);
        }

        public override string GetDerivedEntityTypeExactName(string collectionName, string entityTypeName)
        {
            IEdmEntitySet entitySet;
            IEdmSingleton singleton;
            IEdmEntityType entityType;
            if (TryGetEntitySet(collectionName, out entitySet))
            {
                entityType = (_model.FindDirectlyDerivedTypes(entitySet.EntityType())
                    .SingleOrDefault(x => Utils.NamesMatch((x as IEdmEntityType).Name, entityTypeName, _session.Pluralizer)) as IEdmEntityType);
                if (entityType != null)
                    return entityType.Name;
            }
            else if (TryGetSingleton(collectionName, out singleton))
            {
                entityType = (_model.FindDirectlyDerivedTypes(singleton.EntityType())
                    .SingleOrDefault(x => Utils.NamesMatch((x as IEdmEntityType).Name, entityTypeName, _session.Pluralizer)) as IEdmEntityType);
                if (entityType != null)
                    return entityType.Name;
            }

            throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));
        }

        public override string GetEntityTypeExactName(string entityTypeName)
        {
            var entityType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, entityTypeName, _session.Pluralizer));
            if (entityType != null)
                return entityType.Name;
            
            throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));
        }

        public override IEnumerable<string> GetStructuralPropertyNames(string entitySetName)
        {
            return GetEntityType(entitySetName).StructuralProperties().Select(x => x.Name);
        }

        public override bool HasStructuralProperty(string entitySetName, string propertyName)
        {
            return GetEntityType(entitySetName).StructuralProperties().Any(x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));
        }

        public override string GetStructuralPropertyExactName(string entitySetName, string propertyName)
        {
            return GetStructuralProperty(entitySetName, propertyName).Name;
        }

        public override bool HasNavigationProperty(string entitySetName, string propertyName)
        {
            return GetEntityType(entitySetName).NavigationProperties().Any(x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));
        }

        public override string GetNavigationPropertyExactName(string entitySetName, string propertyName)
        {
            return GetNavigationProperty(entitySetName, propertyName).Name;
        }

        public override string GetNavigationPropertyPartnerName(string entitySetName, string propertyName)
        {
            return (GetNavigationProperty(entitySetName, propertyName).Partner.DeclaringType as IEdmEntityType).Name;
        }

        public override bool IsNavigationPropertyMultiple(string entitySetName, string propertyName)
        {
            return GetNavigationProperty(entitySetName, propertyName).Partner.TargetMultiplicity() == EdmMultiplicity.Many;
        }

        public override IEnumerable<string> GetDeclaredKeyPropertyNames(string entitySetName)
        {
            var entityType = GetEntityType(entitySetName);
            while (entityType.DeclaredKey == null && entityType.BaseEntityType() != null)
            {
                entityType = entityType.BaseEntityType();
            }

            if (entityType.DeclaredKey == null)
                return new string[] { };

            return entityType.DeclaredKey.Select(x => x.Name);
        }

        public override string GetFunctionExactName(string functionName)
        {
            var function = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).OperationImports()
                    .Where(y => y.IsFunctionImport() && y.Name.Homogenize() == functionName.Homogenize()))
                .SingleOrDefault();

            if (function == null)
                throw new UnresolvableObjectException(functionName,
                    string.Format("Function {0} not found", functionName));

            return function.Name;
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
            entitySet = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));

            return entitySet != null;
        }

        private IEnumerable<IEdmSingleton> GetSingletons()
        {
            return _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).Singletons());
        }

        private IEdmSingleton GetSingleton(string singletonName)
        {
            IEdmSingleton singleton;
            if (TryGetSingleton(singletonName, out singleton))
                return singleton;

            throw new UnresolvableObjectException(singletonName, string.Format("Singleton {0} not found", singletonName));
        }

        private bool TryGetSingleton(string singletonName, out IEdmSingleton singleton)
        {
            singleton = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).Singletons())
                .SingleOrDefault(x => Utils.NamesMatch(x.Name, singletonName, _session.Pluralizer));

            return singleton != null;
        }

        private IEnumerable<IEdmEntityType> GetEntityTypes()
        {
            return _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition && (x as IEdmType).TypeKind == EdmTypeKind.Entity)
                .Select(x => x as IEdmEntityType);
        }

        private IEdmEntityType GetEntityType(string collectionName)
        {
            if (collectionName.Contains("/"))
            {
                var items = collectionName.Split('/');
                collectionName = items.First();
                var derivedTypeName = items.Last();

                var entitySet = GetEntitySets()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (entitySet != null)
                {
                    var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, derivedTypeName, _session.Pluralizer));
                    if (derivedType != null)
                    {
                        if (_model.FindDirectlyDerivedTypes(entitySet.EntityType()).Contains(derivedType))
                            return derivedType;
                    }
                }

                var singleton = GetSingletons()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (singleton != null)
                {
                    var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, derivedTypeName, _session.Pluralizer));
                    if (derivedType != null)
                    {
                        if (_model.FindDirectlyDerivedTypes(singleton.EntityType()).Contains(derivedType))
                            return derivedType;
                    }
                }
            }
            else
            {
                var entitySet = GetEntitySets()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (entitySet != null)
                    return entitySet.EntityType();

                var singleton = GetSingletons()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (singleton != null)
                    return singleton.EntityType();

                var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (derivedType != null)
                {
                    var baseType = GetEntityTypes()
                        .SingleOrDefault(x => _model.FindDirectlyDerivedTypes(x).Contains(derivedType));
                    if (baseType != null && GetEntitySets().SingleOrDefault(x => x.EntityType() == baseType) != null)
                        return derivedType;
                }
            }

            throw new UnresolvableObjectException(collectionName, string.Format("Entity set or singleton {0} not found", collectionName));
        }

        private IEdmStructuralProperty GetStructuralProperty(string collectionName, string propertyName)
        {
            var property = GetEntityType(collectionName).StructuralProperties().SingleOrDefault(
                x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Structural property {0} not found", propertyName));

            return property;
        }

        private IEdmNavigationProperty GetNavigationProperty(string collectionName, string propertyName)
        {
            var property = GetEntityType(collectionName).NavigationProperties().SingleOrDefault(x => Utils.NamesMatch(
                x.Name, propertyName, _session.Pluralizer));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Association {0} not found", propertyName));

            return property;
        }
    }
}