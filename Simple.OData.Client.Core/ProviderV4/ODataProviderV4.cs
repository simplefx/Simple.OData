using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class ProviderMetadataV4 : ProviderMetadata
    {
        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public override IEnumerable<string> GetEntitySetNames()
        {
            return GetEntitySets().Select(x => x.Name);
        }

        public override string GetEntitySetExactName(string entitySetName)
        {
            return GetEntitySet(entitySetName).Name;
        }

        public override string GetEntitySetTypeName(string entitySetName)
        {
            return GetEntityType(entitySetName).Name;
        }

        public override string GetEntitySetTypeNamespace(string entitySetName)
        {
            return GetEntityType(entitySetName).Namespace;
        }

        public override bool EntitySetTypeRequiresOptimisticConcurrencyCheck(string entitySetName)
        {
            return GetEntityType(entitySetName).StructuralProperties()
                .Any(x => x.ConcurrencyMode == EdmConcurrencyMode.Fixed);
        }

        public override string GetDerivedEntityTypeExactName(string entitySetName, string entityTypeName)
        {
            var entitySet = GetEntitySet(entitySetName);
            var entityType = (this.Model.FindDirectlyDerivedTypes(entitySet.EntityType())
                .SingleOrDefault(x => NamesAreEqual((x as IEdmEntityType).Name, entityTypeName)) as IEdmEntityType);

            if (entityType == null)
                throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));

            return entityType.Name;
        }

        public override IEnumerable<string> GetDerivedEntityTypeNames(string entitySetName)
        {
            var entitySet = GetEntitySet(entitySetName);
            return this.Model.FindDirectlyDerivedTypes(entitySet.EntityType())
                .Select(x => (x as IEdmEntityType).Name);
        }

        public override string GetEntityTypeExactName(string entityTypeName)
        {
            var entityType = GetEntityTypes().SingleOrDefault(x => NamesAreEqual(x.Name, entityTypeName));

            if (entityType == null)
                throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));

            return entityType.Name;
        }

        public override IEnumerable<string> GetStructuralPropertyNames(string entitySetName)
        {
            return GetEntityType(entitySetName).StructuralProperties().Select(x => x.Name);
        }

        public override bool HasStructuralProperty(string entitySetName, string propertyName)
        {
            return GetEntityType(entitySetName).StructuralProperties().Any(x => NamesAreEqual(x.Name, propertyName));
        }

        public override string GetStructuralPropertyExactName(string entitySetName, string propertyName)
        {
            return GetStructuralProperty(entitySetName, propertyName).Name;
        }

        public override EdmPropertyType GetStructuralPropertyType(string entitySetName, string propertyName)
        {
            return EdmPropertyType.FromModel(GetStructuralProperty(entitySetName, propertyName).Type);
        }

        public override bool HasNavigationProperty(string entitySetName, string propertyName)
        {
            return GetEntityType(entitySetName).NavigationProperties().Any(x => NamesAreEqual(x.Name, propertyName));
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
            var function = this.Model.SchemaElements
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
            return this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets());
        }

        private IEdmEntitySet GetEntitySet(string entitySetName)
        {
            var entitySet = this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .SingleOrDefault(x => NamesAreEqual(x.Name, entitySetName));

            if (entitySet == null)
                throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));

            return entitySet;
        }

        private IEnumerable<IEdmEntityType> GetEntityTypes()
        {
            return this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition && (x as IEdmType).TypeKind == EdmTypeKind.Entity)
                .Select(x => x as IEdmEntityType);
        }

        private IEdmEntityType GetEntityType(string entitySetName)
        {
            var entitySet = GetEntitySets()
                .SingleOrDefault(x => NamesAreEqual(x.Name, entitySetName));

            if (entitySet == null)
            {
                var entityType = GetEntityTypes().SingleOrDefault(x => NamesAreEqual(x.Name, entitySetName));
                if (entityType != null)
                {
                    var baseType = GetEntityTypes()
                        .SingleOrDefault(x => this.Model.FindDirectlyDerivedTypes(x).Contains(entityType));
                    if (baseType != null && GetEntitySets().SingleOrDefault(x => x.EntityType() == baseType) != null)
                        return entityType;
                }
            }

            if (entitySet == null)
                throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));

            return entitySet.EntityType();
        }

        private IEdmStructuralProperty GetStructuralProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).StructuralProperties().SingleOrDefault(x => NamesAreEqual(x.Name, propertyName));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Structural property {0} not found", propertyName));

            return property;
        }

        private IEdmNavigationProperty GetNavigationProperty(string entitySetName, string propertyName)
        {
            var property = GetEntityType(entitySetName).NavigationProperties().SingleOrDefault(x => NamesAreEqual(x.Name, propertyName));

            if (property == null)
                throw new UnresolvableObjectException(propertyName, string.Format("Association {0} not found", propertyName));

            return property;
        }
    }

    class ODataProviderV4
    {
        public EdmSchema CreateEdmSchema(ProviderMetadata providerMetadata)
        {
            return new EdmSchema(new EdmModelParserV4(providerMetadata.Model as IEdmModel));
        }

        public ProviderMetadataV4 GetMetadata(HttpResponseMessage response, string protocolVersion)
        {
            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                var model = messageReader.ReadMetadataDocument();
                return new ProviderMetadataV4
                {
                    ProtocolVersion = protocolVersion,
                    Model = model,
                };
            }
        }

        public ProviderMetadataV4 GetMetadata(string metadataString, string protocolVersion)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            var model = EdmxReader.Parse(reader);
            return new ProviderMetadataV4
                {
                    ProtocolVersion = protocolVersion,
                    Model = model,
                };
        }
    }
}