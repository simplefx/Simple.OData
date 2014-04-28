using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public sealed class EdmSchema
    {
        public string TypesNamespace { get; set; }
        public string ContainersNamespace { get; set; }
        public EdmEntityType[] EntityTypes { get; private set; }
        public EdmComplexType[] ComplexTypes { get; private set; }
        public EdmAssociation[] Associations { get; private set; }
        public EdmEntityContainer[] EntityContainers { get; private set; }

        public EdmSchema(
            string typesNamespace,
            string containersNamespace,
            IEnumerable<EdmEntityType> entityTypes,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmAssociation> associations,
            IEnumerable<EdmEntityContainer> entityContainers)
        {
            this.TypesNamespace = typesNamespace;
            this.ContainersNamespace = containersNamespace;
            this.EntityTypes = entityTypes.ToArray();
            this.ComplexTypes = complexTypes.ToArray();
            this.Associations = associations.ToArray();
            this.EntityContainers = entityContainers.ToArray();
        }
    }

    public sealed class EdmEntitySet
    {
        public string Name { get; set; }
        public string EntityType { get; set; }
    }

    public sealed class EdmAssociationSet
    {
        public string Name { get; set; }
        public string Association { get; set; }
        public EdmAssociationSetEnd[] End { get; set; }
    }

    public sealed class EdmEntityType
    {
        public string Name { get; set; }
        public EdmEntityType BaseType { get; set; }
        public bool Abstract { get; set; }
        public bool OpenType { get; set; }
        public EdmKey Key { get; set; }
        public EdmProperty[] Properties { get; set; }
        public EdmNavigationProperty[] NavigationProperties { get; set; }

        public static Tuple<bool, EdmEntityType> TryParse(string s, IEnumerable<EdmEntityType> entityTypes)
        {
            var edmEntityType = entityTypes.SingleOrDefault(x => x.Name == s);
            return Tuple.Create(edmEntityType != null, edmEntityType);
        }
    }

    public sealed class EdmComplexType
    {
        public string Name { get; set; }
        public EdmProperty[] Properties { get; set; }

        public static Tuple<bool, EdmComplexType> TryParse(string s, IEnumerable<EdmComplexType> complexTypes)
        {
            var edmComplexType = complexTypes.SingleOrDefault(x => x.Name == s);
            return Tuple.Create(edmComplexType != null, edmComplexType);
        }
    }

    public sealed class EdmAssociation
    {
        public string Name { get; set; }
        public EdmAssociationEnd[] End { get; set; }
        public EdmReferentialConstraint ReferentialConstraint { get; set; }
    }

    public sealed class EdmEntityContainer
    {
        public string Name { get; set; }
        public bool IsDefaulEntityContainer { get; set; }
        public EdmEntitySet[] EntitySets { get; set; }
        public EdmAssociationSet[] AssociationSets { get; set; }
        public EdmFunctionImport[] FunctionImports { get; set; }
    }

    public sealed class EdmProperty
    {
        public string Name { get; set; }
        public EdmPropertyType Type { get; set; }
        public bool Nullable { get; set; }
    }

    public sealed class EdmNavigationProperty
    {
        public string Name { get; set; }
        public string ToRole { get; set; }
        public string FromRole { get; set; }
        public string Relationship { get; set; }
    }

    public sealed class EdmKey
    {
        public string[] Properties { get; set; }
    }

    public sealed class EdmAssociationEnd
    {
        public string Role { get; set; }
        public string Type { get; set; }
        public string Multiplicity { get; set; }
    }

    public sealed class EdmReferentialConstraint
    {
        public EdmReferentialConstraintEnd Principal { get; set; }
        public EdmReferentialConstraintEnd Dependent { get; set; }
    }

    public sealed class EdmReferentialConstraintEnd
    {
        public string Role { get; set; }
        public string[] Properties { get; set; }
    }

    public sealed class EdmAssociationSetEnd
    {
        public string Role { get; set; }
        public string EntitySet { get; set; }
    }

    public sealed class EdmFunctionImport
    {
        public string Name { get; set; }
        public string HttpMethod { get; set; }
        public string EntitySet { get; set; }
        public EdmPropertyType ReturnType { get; set; }
        public EdmParameter[] Parameters { get; set; }
    }

    public sealed class EdmParameter
    {
        public string Name { get; set; }
        public EdmPropertyType Type { get; set; }
        public string Mode { get; set; }
    }
}
