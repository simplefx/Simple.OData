using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class ProviderMetadataV3 : ProviderMetadata
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
            var entityType = (this.Model.FindDirectlyDerivedTypes(entitySet.ElementType)
                .SingleOrDefault(x => NamesAreEqual((x as IEdmEntityType).Name, entityTypeName)) as IEdmEntityType);

            if (entityType == null)
                throw new UnresolvableObjectException(entityTypeName, string.Format("Entity type {0} not found", entityTypeName));

            return entityType.Name;
        }

        public override IEnumerable<string> GetDerivedEntityTypeNames(string entitySetName)
        {
            var entitySet = GetEntitySet(entitySetName);
            return this.Model.FindDirectlyDerivedTypes(entitySet.ElementType)
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
            var function = this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).FunctionImports()
                    .Where(y => y.Name.Homogenize() == functionName.Homogenize()))
                .SingleOrDefault();

            if (function == null)
                throw new UnresolvableObjectException(functionName, string.Format("Function {0} not found", functionName));

            return function.Name;
        }

        public override string CreateEntry(string entityTypeNamespace, string entityTypeName,
            IDictionary<string, object> row)
        {
            // TODO: check dispose
            var message = new ODataV3RequestMessage(null, null);
            var messageWriter = new ODataMessageWriter(message);
            var entryWriter = messageWriter.CreateODataEntryWriter();
            var entry = new Microsoft.Data.OData.ODataEntry();
            entry.Properties = row.Select(x => new ODataProperty() { Name = x.Key, Value = x.Value });
            entryWriter.WriteStart(entry);
            entryWriter.WriteEnd();

            var text = StreamToString(message.GetStream());
            return text;
        }

        public async override Task<IEnumerable<IDictionary<string, object>>> GetEntriesAsync(HttpResponseMessage response)
        {
            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response), new ODataMessageReaderSettings()))
            {
                var entries = new List<IDictionary<string, object>>();
                var feedReader = messageReader.CreateODataFeedReader();
                while (feedReader.Read())
                {
                    switch (feedReader.State)
                    {
                        case ODataReaderState.FeedStart:
                        case ODataReaderState.EntryStart:
                        case ODataReaderState.FeedEnd:
                        default:
                            break;

                        case ODataReaderState.EntryEnd:
                    entries.Add((feedReader.Item as Microsoft.Data.OData.ODataEntry).Properties.ToDictionary(x => x.Name, x => x.Value));
                            break;
                    }
                }
                return entries;
            }
        }

        public async override Task<IDictionary<string, object>> GetEntryAsync(HttpResponseMessage response)
        {
            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response), new ODataMessageReaderSettings()))
            {
                var entryReader = messageReader.CreateODataEntryReader();
                while (entryReader.Read())
                {
                    switch (entryReader.State)
                    {
                        case ODataReaderState.FeedStart:
                        case ODataReaderState.EntryStart:
                        case ODataReaderState.FeedEnd:
                        default:
                            break;

                        case ODataReaderState.EntryEnd:
                            return (entryReader.Item as Microsoft.Data.OData.ODataEntry).Properties.ToDictionary(x => x.Name, x => x.Value);
                    }
                }
                return null;
            }
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
                    if (baseType != null && GetEntitySets().SingleOrDefault(x => x.ElementType == baseType) != null)
                        return entityType;
                }
            }

            if (entitySet == null)
                throw new UnresolvableObjectException(entitySetName, string.Format("Entity set {0} not found", entitySetName));

            return entitySet.ElementType;
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
                throw new UnresolvableObjectException(propertyName, string.Format("Navigation property {0} not found", propertyName));

            return property;
        }
    }

    class ODataProviderV3
    {
        public EdmSchema CreateEdmSchema(ProviderMetadata providerMetadata)
        {
            return new EdmSchema(new EdmModelParserV3(providerMetadata.Model as IEdmModel));
        }

        public ProviderMetadataV3 GetMetadata(HttpResponseMessage response, string protocolVersion)
        {
            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response)))
            {
                var model = messageReader.ReadMetadataDocument();
                return new ProviderMetadataV3
                {
                    ProtocolVersion = protocolVersion,
                    Model = model,
                };
            }
        }

        public ProviderMetadataV3 GetMetadata(string metadataString, string protocolVersion)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            var model = EdmxReader.Parse(reader);
            return new ProviderMetadataV3
            {
                ProtocolVersion = protocolVersion,
                Model = model,
            };
        }
    }
}