using System;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    public abstract partial class EdmPropertyType
    {
        public static EdmPropertyType FromODataType(IEdmTypeReference type)
        {
            switch (type.Definition.TypeKind)
            {
                case EdmTypeKind.Primitive:
                    return new EdmPrimitivePropertyType
                    {
                        Type = EdmType.FromODataType(type),
                    };

                case EdmTypeKind.Complex:
                    return new EdmComplexPropertyType()
                    {
                        Type = EdmComplexType.FromODataType(type.Definition as IEdmComplexType),
                    };

                default:
                    throw new NotSupportedException();
            }
        }
    }
}