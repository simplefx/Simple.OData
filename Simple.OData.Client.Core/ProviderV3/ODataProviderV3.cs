using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Data.Edm;
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

        public override bool HasNavigationProperty(string entitySetName, string propertyName)
        {
            return GetNavigationProperties(entitySetName).Any(
                x => x.Name == propertyName || x.Name == propertyName.Singularize() || x.Name == propertyName.Pluralize());
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
            return GetNavigationProperty(entitySetName, propertyName).Partner.Multiplicity() == EdmMultiplicity.Many;
        }

        public override string GetFunctionActualName(string functionName)
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

        private IEnumerable<IEdmNavigationProperty> GetNavigationProperties(string entitySetName)
        {
            return this.Model.SchemaElements
                .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                .Single(x => x.Name == entitySetName).ElementType
                .NavigationProperties();
        }

        private IEdmNavigationProperty GetNavigationProperty(string entitySetName, string propertyName)
        {
            var property = GetNavigationProperties(entitySetName).SingleOrDefault(x => 
                x.Name.Homogenize() == propertyName.Homogenize() ||
                x.Name.Homogenize() == propertyName.Singularize().Homogenize() ||
                x.Name.Homogenize() == propertyName.Pluralize().Homogenize());

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
    }
}