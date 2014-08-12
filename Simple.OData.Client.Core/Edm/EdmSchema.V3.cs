using System.Linq;
using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmEntityType
    {
        public static EdmEntityType FromODataType(IEdmEntityType type)
        {
            return new EdmEntityType
            {
                Namespace = type.Namespace,
                Name = type.Name,
                Properties = type.StructuralProperties().Select(EdmProperty.FromODataProperty).ToArray(),
            };
        }
    }

    public sealed partial class EdmComplexType
    {
        public static EdmComplexType FromODataType(IEdmComplexType type)
        {
            return new EdmComplexType
            {
                Namespace = type.Namespace,
                Name = type.Name,
                Properties = type.StructuralProperties().Select(EdmProperty.FromODataProperty).ToArray(),
            };
        }
    }

    public sealed partial class EdmProperty
    {
        public static EdmProperty FromODataProperty(IEdmStructuralProperty property)
        {
            return new EdmProperty()
            {
                Name = property.Name,
                Type = EdmPropertyType.FromODataType(property.Type),
                Nullable = property.Type.IsNullable
            };
        }
    }
}