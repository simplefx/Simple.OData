using System;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    public abstract partial class EdmPropertyType
    {
        public static EdmPropertyType FromModel(IEdmTypeReference type)
        {
            switch (type.Definition.TypeKind)
            {
                case EdmTypeKind.Entity:
                    return new EdmEntityPropertyType
                    {
                        Type = EdmEntityType.FromModel(type.Definition as IEdmEntityType),
                    };

                case EdmTypeKind.Complex:
                    return new EdmComplexPropertyType
                    {
                        Type = EdmComplexType.FromModel(type.Definition as IEdmComplexType),
                    };

                case EdmTypeKind.Primitive:
                    return new EdmPrimitivePropertyType
                    {
                        Type = EdmType.FromModel(type),
                    };

                case EdmTypeKind.Collection:
                    return new EdmCollectionPropertyType
                    {
                        BaseType = EdmPropertyType.FromModel((type.Definition as IEdmCollectionType).ElementType),
                    };

                default:
                    throw new NotSupportedException();
            }
        }
    }
}