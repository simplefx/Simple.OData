using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;
using Simple.OData.Client.Extensions;

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
            return GetEntitySet(collectionName).Name;
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
            IEdmEntityType entityType;
            if (TryGetEntitySet(collectionName, out entitySet))
            {
                entityType = (_model.FindDirectlyDerivedTypes(entitySet.ElementType)
                    .SingleOrDefault(x => Utils.NamesMatch((x as IEdmEntityType).Name, entityTypeName, _session.Pluralizer)) as IEdmEntityType);
                if (entityType != null)
                    return entityType.Name;
            }
            else if (TryGetEntityType(entityTypeName, out entityType))
            {
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
            var navigationProperty = GetNavigationProperty(entitySetName, propertyName);
            var entityType = navigationProperty.Type.Definition.TypeKind == EdmTypeKind.Collection
                ? (navigationProperty.Type.Definition as IEdmCollectionType).ElementType.Definition as IEdmEntityType
                : navigationProperty.Type.Definition as IEdmEntityType;
            return entityType.Name;
        }

        public override bool IsNavigationPropertyMultiple(string entitySetName, string propertyName)
        {
            return GetNavigationProperty(entitySetName, propertyName).Partner.Multiplicity() == EdmMultiplicity.Many;
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
                .SelectMany(x => (x as IEdmEntityContainer).FunctionImports()
                    .Where(y => y.Name.Homogenize() == functionName.Homogenize()))
                .SingleOrDefault();

            if (function == null)
                throw new UnresolvableObjectException(functionName, string.Format("Function {0} not found", functionName));

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
            if (entitySetName.Contains("/"))
                entitySetName = entitySetName.Split('/').First();

            entitySet = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));

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
                        .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                    if (entitySet != null)
                    {
                        var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, derivedTypeName, _session.Pluralizer));
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
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
                if (entitySet != null)
                {
                    entityType = entitySet.ElementType;
                    return true;
                }

                var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, collectionName, _session.Pluralizer));
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

        private IEdmStructuralProperty GetStructuralProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).StructuralProperties().SingleOrDefault(
                x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Structural property {0} not found", propertyName));

            return property;
        }

        private IEdmNavigationProperty GetNavigationProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).NavigationProperties().SingleOrDefault(
                x => Utils.NamesMatch(x.Name, propertyName, _session.Pluralizer));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Navigation property {0} not found", propertyName));

            return property;
        }
    }
}