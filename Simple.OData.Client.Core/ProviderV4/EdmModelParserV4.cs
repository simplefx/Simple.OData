using System;
using System.Linq;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    class EdmModelParserV4 : IEdmModelParser
    {
        private IEdmModel _model;

        public EdmModelParserV4(IEdmModel model)
        {
            _model = model;
        }

        public string[] SupportedProtocols
        {
            get
            {
                return new[] { "4.0" };
            }
        }

        public EdmEntityType[] EntityTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Entity)
                    .Select(x => EdmEntityType.FromModel(x as IEdmEntityType)).ToArray();
            }
        }

        public EdmComplexType[] ComplexTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Complex)
                    .Select(x => EdmComplexType.FromModel(x as IEdmComplexType)).ToArray();
            }
        }

        public EdmEnumType[] EnumTypes
        {
            get
            {
                return _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition &&
                        (x as IEdmSchemaType).TypeKind == EdmTypeKind.Enum)
                    .Select(x => EdmEnumType.FromModel(x as IEdmEnumType)).ToArray();
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
                return _model.SchemaElements.Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer).Select(x =>
                    new EdmEntityContainer()
                    {
                        Namespace = x.Namespace,
                        Name = x.Name,
                        EntitySets = (x as IEdmEntityContainer).EntitySets().Select(EdmEntitySet.FromModel).ToArray(),
                        FunctionImports = (x as IEdmEntityContainer).OperationImports().Select(EdmFunctionImport.FromModel).ToArray(),
                        IsDefaulEntityContainer = false,
                    }).ToArray();
            }
        }
    }
}