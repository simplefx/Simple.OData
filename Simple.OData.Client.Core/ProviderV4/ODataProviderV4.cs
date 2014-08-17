using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
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

        public override bool HasNavigationProperty(string entitySetName, string propertyName)
        {
            return GetNavigationProperties(entitySetName).Any(x => x.Name == propertyName);
        }

        public override string GetNavigationPropertyActualName(string entitySetName, string propertyName)
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

        public override string GetFunctionActualName(string functionName)
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

        private IEnumerable<IEdmNavigationProperty> GetNavigationProperties(string entitySetName)
        {
            return this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .Single(x => x.Name == entitySetName).EntityType()
                .NavigationProperties();
        }

        private IEdmNavigationProperty GetNavigationProperty(string entitySetName, string propertyName)
        {
            var property = GetNavigationProperties(entitySetName).Single(x => x.Name.Homogenize() == propertyName.Homogenize());

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
    }
}