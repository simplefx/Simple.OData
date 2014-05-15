using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    internal class EdmSchemaParser
    {
        public string TypesNamespace { get; private set; }
        public string ContainersNamespace { get; private set; }
        public IEnumerable<EdmEntityType> EntityTypes { get; private set; }
        public IEnumerable<EdmComplexType> ComplexTypes { get; private set; }
        public IEnumerable<EdmEnumType> EnumTypes { get; private set; }
        public IEnumerable<EdmAssociation> Associations { get; private set; }
        public IEnumerable<EdmEntityContainer> EntityContainers { get; private set; }

        public EdmSchemaParser()
        {
            this.EntityTypes = new List<EdmEntityType>();
            this.ComplexTypes = new List<EdmComplexType>();
            this.EnumTypes = new List<EdmEnumType>();
            this.Associations = new List<EdmAssociation>();
            this.EntityContainers = new List<EdmEntityContainer>();
        }

        public EdmSchema ParseSchema(XElement element)
        {
            var schemaRoot = element.Descendants(null, "Schema");

            this.TypesNamespace = schemaRoot
                .Where(x => x.Descendants(null, "EntityType").Any()).FirstOrDefault().Attribute("Namespace").Value;
            this.ContainersNamespace = schemaRoot
                .Where(x => x.Descendants(null, "EntityContainer").Any()).FirstOrDefault().Attribute("Namespace").Value;

            ParseEnumTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EnumType")));
            ParseComplexTypes(schemaRoot.SelectMany(x => x.Descendants(null, "ComplexType")));
            ParseEntityTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EntityType")));
            ParseAssociations(schemaRoot.SelectMany(x => x.Descendants(null, "Association")));
            ParseEntityContainers(schemaRoot.SelectMany(x => x.Descendants(null, "EntityContainer")));

            return new EdmSchema(this);
        }

        private void ParseEnumTypes(IEnumerable<XElement> elements)
        {
            Func<XElement, string> GetEnumTypeName = x => String.Format("{0}.{1}", this.TypesNamespace, x.Attribute("Name").Value);
            this.EnumTypes = (from e in elements select new EdmEnumType
            {
                Name = GetEnumTypeName(e),
                UnderlyingType = ParseStringAttribute(e.Attribute("UnderlyingType")),
                IsFlags = ParseBooleanAttribute(e.Attribute("IsFlags"))
            }).ToList();
            foreach (var element in elements)
            {
                var enumType = this.EnumTypes.Single(x => x.Name == GetEnumTypeName(element));
                enumType.Members = (from m in element.Descendants(null, "Member")
                                          select ParseEnumMember(m)).ToArray();
                long currentValue = 0;
                foreach (var member in enumType.Members)
                {
                    long value;
                    member.EvaluatedValue = long.TryParse(member.Value, out value) ? value : currentValue;
                    currentValue = enumType.IsFlags
                        ? currentValue == 0 ? 1 : currentValue*2
                        : ++currentValue;
                }
            }
        }

        private void ParseComplexTypes(IEnumerable<XElement> elements)
        {
            Func<XElement, string> GetComplexTypeName = x => String.Format("{0}.{1}", this.TypesNamespace, x.Attribute("Name").Value);
            this.ComplexTypes = (from e in elements select new EdmComplexType { Name = GetComplexTypeName(e) }).ToList();
            foreach (var element in elements)
            {
                var complexType = this.ComplexTypes.Single(x => x.Name == GetComplexTypeName(element));
                complexType.Properties = (from p in element.Descendants(null, "Property")
                                          select ParseProperty(p)).ToArray();
            }
        }

        private void ParseEntityTypes(IEnumerable<XElement> elements)
        {
            var results = from e in elements
                          select new
                          {
                              EntityType = new EdmEntityType()
                              {
                                  Name = e.Attribute("Name").Value,
                                  Abstract = ParseBooleanAttribute(e.Attribute("Abstract")),
                                  OpenType = ParseBooleanAttribute(e.Attribute("OpenType")),
                                  Key = (from k in e.Descendants(null, "Key")
                                         select ParseKey(k)).SingleOrDefault(),
                                  Properties = (from p in e.Descendants(null, "Property")
                                                select ParseProperty(p)).ToArray(),
                                  NavigationProperties = (from p in e.Descendants(null, "NavigationProperty")
                                                select ParseNavigationProperty(p)).ToArray(),
                              },
                              BaseType = ParseStringAttribute(e.Attribute("BaseType")),
                          };
            this.EntityTypes = from r in results
                   select new EdmEntityType()
                   {
                       Name = r.EntityType.Name,
                       BaseType = String.IsNullOrEmpty(r.BaseType) ? null : results.Single(y => y.EntityType.Name == r.BaseType.Split('.').Last()).EntityType,
                       Abstract = r.EntityType.Abstract,
                       OpenType = r.EntityType.OpenType,
                       Key = r.EntityType.Key,
                       Properties = r.EntityType.Properties,
                       NavigationProperties = r.EntityType.NavigationProperties,
                   };
        }

        private void ParseAssociations(IEnumerable<XElement> elements)
        {
            this.Associations = from e in elements
                   select new EdmAssociation()
                   {
                       Name = e.Attribute("Name").Value,
                       End = (from p in e.Descendants(null, "End")
                              select new EdmAssociationEnd()
                              {
                                  Role = p.Attribute("Role").Value,
                                  Type = p.Attribute("Type").Value,
                                  Multiplicity = p.Attribute("Multiplicity").Value,
                              }).ToArray(),
                       ReferentialConstraint = (from c in e.Descendants(null, "ReferentialConstraint")
                                                select new EdmReferentialConstraint()
                                                {
                                                    Principal = (from r in c.Descendants(null, "Principal")
                                                                 select new EdmReferentialConstraintEnd()
                                                                 {
                                                                     Role = r.Attribute("Role").Value,
                                                                     Properties = (from p in r.Descendants(null, "PropertyRef")
                                                                                   select p.Attribute("Name").Value).ToArray(),
                                                                 }
                                                        ).Single(),
                                                    Dependent = (from r in c.Descendants(null, "Dependent")
                                                                 select new EdmReferentialConstraintEnd()
                                                                 {
                                                                     Role = r.Attribute("Role").Value,
                                                                     Properties = (from p in r.Descendants(null, "PropertyRef")
                                                                                   select p.Attribute("Name").Value).ToArray(),
                                                                 }
                                                        ).Single(),
                                                }).SingleOrDefault(),
                   };
        }

        private void ParseEntityContainers(IEnumerable<XElement> elements)
        {
            this.EntityContainers = from e in elements
                   select new EdmEntityContainer()
                   {
                       Name = e.Attribute("Name").Value,
                       IsDefaulEntityContainer = ParseBooleanAttribute(e.Attribute("m", "IsDefaultEntityContainer")),
                       EntitySets = (from s in e.Descendants(null, "EntitySet")
                                     select new EdmEntitySet()
                                     {
                                         Name = s.Attribute("Name").Value,
                                         EntityType = s.Attribute("EntityType").Value,
                                     }).ToArray(),
                       AssociationSets = (from s in e.Descendants(null, "AssociationSet")
                                          select new EdmAssociationSet()
                                          {
                                              Name = s.Attribute("Name").Value,
                                              Association = s.Attribute("Association").Value,
                                              End = (from n in s.Descendants(null, "End")
                                                     select new EdmAssociationSetEnd()
                                                     {
                                                         Role = n.Attribute("Role").Value,
                                                         EntitySet = n.Attribute("EntitySet").Value,
                                                     }).ToArray(),
                                          }).ToArray(),
                       FunctionImports = (from s in e.Descendants(null, "FunctionImport")
                                          select new EdmFunctionImport()
                                          {
                                              Name = s.Attribute("Name").Value,
                                              HttpMethod = ParseStringAttribute(s.Attribute("m", "HttpMethod")),
                                              ReturnType = ParseType(s.Attribute("ReturnType")),
                                              EntitySet = ParseStringAttribute(s.Attribute("EntitySet")),
                                              Parameters = (from p in s.Descendants(null, "Parameter")
                                                            select new EdmParameter()
                                                            {
                                                                Name = p.Attribute("Name").Value,
                                                                Type = EdmPropertyType.Parse(p.Attribute("Type").Value, 
                                                                        this.EntityTypes, this.ComplexTypes, this.EnumTypes),
                                                            }).ToArray(),
                                          }).ToArray(),
                   };

        }

        private EdmProperty ParseProperty(XElement element)
        {
            return new EdmProperty
            {
                Name = element.Attribute("Name").Value,
                Type = EdmPropertyType.Parse(element.Attribute("Type").Value, this.EntityTypes, this.ComplexTypes, this.EnumTypes),
                Nullable = ParseBooleanAttribute(element.Attribute("Nullable"), true),
            };
        }

        private EdmNavigationProperty ParseNavigationProperty(
            XElement element)
        {
            return new EdmNavigationProperty
            {
                Name = element.Attribute("Name").Value,
                ToRole = element.Attribute("ToRole").Value,
                FromRole = element.Attribute("FromRole").Value,
                Relationship = element.Attribute("Relationship").Value,
            };
        }

        private EdmPropertyType ParseType(XAttribute attribute)
        {
            if (attribute == null || attribute.Value == null)
                return null;

            var attritbuteValue = ParseStringAttribute(attribute);
            return EdmPropertyType.Parse(attritbuteValue, this.EntityTypes, this.ComplexTypes, this.EnumTypes);
        }

        private EdmKey ParseKey(XElement element)
        {
            return new EdmKey()
            {
                Properties = (from p in element.Descendants(null, "PropertyRef")
                              select p.Attribute("Name").Value).ToArray()
            };
        }

        private EdmEnumMember ParseEnumMember(XElement element)
        {
            return new EdmEnumMember
            {
                Name = element.Attribute("Name").Value,
                Value = ParseStringAttribute(element.Attribute("Value")),
            };
        }

        private bool ParseBooleanAttribute(XAttribute attribute, bool @default = false)
        {
            bool result = @default;
            if (attribute != null)
            {
                Boolean.TryParse(attribute.Value, out result);
            }
            return result;
        }

        private string ParseStringAttribute(XAttribute attribute, string @default = null)
        {
            return attribute == null ? @default : attribute.Value;
        }
    }
}