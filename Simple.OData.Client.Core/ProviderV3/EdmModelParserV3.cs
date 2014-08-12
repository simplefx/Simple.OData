using System;
using System.Linq;
using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    class EdmModelParserV3 : IEdmModelParser
    {
        private IEdmModel _model;

        public EdmModelParserV3(IEdmModel model)
        {
            _model = model;
        }

        public string[] SupportedProtocols
        {
            get
            {
                return new[] { "1.0", "2.0", "3.0" };
            }
        }

        public EdmEntityType[] EntityTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Entity)
                    .Select(x => EdmEntityType.FromODataType(x as IEdmEntityType)).ToArray();
            }
        }

        public EdmComplexType[] ComplexTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Complex)
                    .Select(x => EdmComplexType.FromODataType(x as IEdmComplexType)).ToArray();
            }
        }

        public EdmEnumType[] EnumTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Enum)
                    .Select(x => EdmEnumType.FromODataType(x as IEdmEnumType)).ToArray();
            }
        }

        public EdmAssociation[] Associations
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public EdmEntityContainer[] EntityContainers
        {
            get
            {
                return _model.EntityContainers().Select(x =>
                    new EdmEntityContainer()
                    {
                        Namespace = x.Namespace,
                        Name = x.Name,
                    }).ToArray();
            }
        }
    }
}