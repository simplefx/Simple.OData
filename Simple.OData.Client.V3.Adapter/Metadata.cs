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

        public override IEnumerable<string> GetEntityCollectionNames()
        {
            return GetEntitySets().Select(x => x.Name);
        }

        public override string GetEntityCollectionExactName(string collectionName)
        {
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
            var entitySet = GetEntitySet(collectionName);
            var entityType = (_model.FindDirectlyDerivedTypes(entitySet.ElementType)
                .SingleOrDefault(x => Utils.NamesMatch((x as IEdmEntityType).Name, entityTypeName, _session.Pluralizer)) as IEdmEntityType);

            if (entityType != null)
                return entityType.Name;

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
            if (entitySetName.Contains("/"))
                entitySetName = entitySetName.Split('/').First();

            var entitySet = _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));

            if (entitySet == null)
                throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));

            return entitySet;
        }

        private IEnumerable<IEdmEntityType> GetEntityTypes()
        {
            return _model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition && (x as IEdmType).TypeKind == EdmTypeKind.Entity)
                .Select(x => x as IEdmEntityType);
        }

        private IEdmEntityType GetEntityType(string entitySetName)
        {
            if (entitySetName.Contains("/"))
            {
                var items = entitySetName.Split('/');
                entitySetName = items.First();
                var derivedTypeName = items.Last();

                var entitySet = GetEntitySets()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));
                if (entitySet != null)
                {
                    var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, derivedTypeName, _session.Pluralizer));
                    if (derivedType != null)
                    {
                        if (_model.FindDirectlyDerivedTypes(entitySet.ElementType).Contains(derivedType))
                            return derivedType;
                    }
                }
            }
            else
            {
                var entitySet = GetEntitySets()
                    .SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));
                if (entitySet != null)
                    return entitySet.ElementType;

                var derivedType = GetEntityTypes().SingleOrDefault(x => Utils.NamesMatch(x.Name, entitySetName, _session.Pluralizer));
                if (derivedType != null)
                {
                    var baseType = GetEntityTypes()
                        .SingleOrDefault(x => _model.FindDirectlyDerivedTypes(x).Contains(derivedType));
                    if (baseType != null && GetEntitySets().SingleOrDefault(x => x.ElementType == baseType) != null)
                        return derivedType;
                }
            }

            throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));
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