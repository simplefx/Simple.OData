using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.Edm
{
    public sealed class EdmSchema
    {
        public string Namespace { get; set; }
        public EdmEntityType[] EntityTypes { get; private set; }
        public EdmComplexType[] ComplexTypes { get; private set; }
        public EdmAssociation[] Associations { get; private set; }
        public EdmEntityContainer[] EntityContainers { get; private set; }

        public EdmSchema(
            IEnumerable<EdmEntityType> entityTypes,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmAssociation> associations,
            IEnumerable<EdmEntityContainer> entityContainers)
        {
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
        public EdmProperty[] Properties { get; set; }
        public EdmKey Key { get; set; }
    }

    public sealed class EdmComplexType
    {
        public string Name { get; set; }
        public EdmProperty[] Properties { get; set; }
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

    public abstract class EdmPropertyType
    {
        public static EdmPropertyType Parse(string s, IEnumerable<EdmComplexType> complexTypes)
        {
            var result = EdmType.TryParse(s);
            if (result.Item1)
            {
                return new EdmPrimitivePropertyType { Type = result.Item2 };
            }
            else
            {
                return new EdmComplexPropertyType { Type = complexTypes.SingleOrDefault(x => x.Name == s) };
            }
        }
    }

    public class EdmPrimitivePropertyType : EdmPropertyType
    {
        public EdmType Type { get; set; }
    }

    public class EdmComplexPropertyType : EdmPropertyType
    {
        public EdmComplexType Type { get; set; }
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
        public string EntitySet { get; set; }
        public string ReturnType { get; set; }
        public string HttpMethod { get; set; }
        public EdmParameter[] Parameters { get; set; }
    }

    public sealed class EdmParameter
    {
        public string Name { get; set; }
        public EdmType Type { get; set; }
        public string Mode { get; set; }
    }
}
