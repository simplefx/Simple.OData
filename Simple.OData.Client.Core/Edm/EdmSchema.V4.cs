using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    public sealed partial class EdmEntitySet
    {
        public static EdmEntitySet FromModel(IEdmEntitySet entitySet)
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
        public static EdmEntityType FromModel(IEdmEntityType type)
        {
            return new EdmEntityType
            {
                Namespace = type.Namespace,
                Name = type.Name,
                BaseType = FromModel(type.BaseType),
                Abstract = type.IsAbstract,
                OpenType = type.IsOpen,
                Key = EdmKey.FromModel(type.DeclaredKey),
                Properties = type.StructuralProperties().Select(EdmProperty.FromModel).ToArray(),
                NavigationProperties = type.NavigationProperties().Select(EdmNavigationProperty.FromModel).ToArray(),
            };
        }

        public static EdmEntityType FromModel(IEdmStructuredType type)
        {
            return type == null ? null : new EdmEntityType
            {
                BaseType = FromModel(type.BaseType),
                Abstract = type.IsAbstract,
                OpenType = type.IsOpen,
                Properties = type.StructuralProperties().Select(EdmProperty.FromModel).ToArray(),
            };
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
                Nullable = property.Type.IsNullable
            };
        }
    }

    public sealed partial class EdmNavigationProperty
    {
        public static EdmNavigationProperty FromModel(IEdmNavigationProperty property)
        {
            return new EdmNavigationProperty()
            {
                Name = property.Name, // TODO
                FromRole = property.Name,
                ToRole = property.Partner.Name,
                Relationship = "", // TODO
            };
        }
    }

    public sealed partial class EdmKey
    {
        public static EdmKey FromModel(IEnumerable<IEdmStructuralProperty> properties)
        {
            return properties == null ? null : new EdmKey()
            {
                Properties = properties.Select(x => x.Name).ToArray()
            };
        }
    }

    public sealed partial class EdmFunctionImport
    {
        public static EdmFunctionImport FromModel(IEdmOperationImport function)
        {
            return new EdmFunctionImport()
            {
                Name = function.Name,
                HttpMethod = "", // TODO
                ReturnType = EdmPropertyType.FromModel(function.Operation.ReturnType),
                EntitySet = function.EntitySet.ToString(), // TODO
                Parameters = function.Operation.Parameters.Select(EdmParameter.FromModel).ToArray(),
            };
        }
    }

    public sealed partial class EdmParameter
    {
        public static EdmParameter FromModel(IEdmOperationParameter parameter)
        {
            return new EdmParameter
            {
                Name = parameter.Name,
                Type = EdmPropertyType.FromModel(parameter.Type),
                Mode = "", // TODO
            };
        }
    }
}
