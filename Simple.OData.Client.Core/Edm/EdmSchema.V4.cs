using System.Linq;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmEntitySet
    {
        public static EdmEntitySet FromODataEntitySet(IEdmEntitySet entitySet)
        {
            return new EdmEntitySet
            {
                Name = entitySet.Name,
                EntityType = GetEntityTypeName(entitySet.Type),
            };
        }

        private static string GetEntityTypeName(IEdmType type)
        {
            var typeName = type.FullTypeName();
            const string collectionPrefix = "Collection(";
            if (typeName.StartsWith(collectionPrefix))
                return typeName.Substring(collectionPrefix.Length, typeName.Length - collectionPrefix.Length - 1);
            else
                return typeName;
        }
    }

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
