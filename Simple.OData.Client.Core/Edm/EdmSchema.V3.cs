using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmEntitySet
    {
        public static EdmEntitySet FromModel(IEdmEntitySet entitySet)
        {
            return new EdmEntitySet
            {
                Name = entitySet.Name,
                EntityType = entitySet.ElementType.Name,
            };
        }
    }

    public sealed partial class EdmEntityType
    {
        public static EdmEntityType FromModel(IEdmEntityType type)
        {
            return new EdmEntityType
            {
                Namespace = type.Namespace,
                Name = type.Name,
                BaseType = FromModel(type.BaseType),
                Abstract = type.IsAbstract,
                OpenType = type.IsOpen,
                Properties = type.DeclaredProperties.Where(x => x.PropertyKind == EdmPropertyKind.Structural)
                    .Select(x => EdmProperty.FromModel(x as IEdmStructuralProperty)).ToArray(),
            };
        }

        public static EdmEntityType FromModel(IEdmStructuredType type)
        {
            return type == null ? null : FromModel(type as IEdmEntityType);
        }
    }

    public sealed partial class EdmComplexType
    {
        public static EdmComplexType FromModel(IEdmComplexType type)
        {
            return new EdmComplexType
            {
                Namespace = type.Namespace,
                Name = type.Name,
                Properties = type.StructuralProperties().Select(EdmProperty.FromModel).ToArray(),
            };
        }
    }

    public sealed partial class EdmProperty
    {
        public static EdmProperty FromModel(IEdmStructuralProperty property)
        {
            return new EdmProperty()
            {
                Name = property.Name,
                Type = EdmPropertyType.FromModel(property.Type),
                Nullable = property.Type.IsNullable,
                ConcurrencyMode = property.ConcurrencyMode.ToString(),
            };
        }
    }
}