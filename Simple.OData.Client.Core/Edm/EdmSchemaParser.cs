using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class EdmSchemaParser
    {
        public IEnumerable<EdmEntityType> EntityTypes { get; private set; }
        public IEnumerable<EdmComplexType> ComplexTypes { get; private set; }
        public IEnumerable<EdmEnumType> EnumTypes { get; private set; }
        public IEnumerable<EdmEntityContainer> EntityContainers { get; private set; }

        public EdmSchemaParser()
        {
            this.EntityTypes = new List<EdmEntityType>();
            this.ComplexTypes = new List<EdmComplexType>();
            this.EnumTypes = new List<EdmEnumType>();
            this.EntityContainers = new List<EdmEntityContainer>();
        }

        public EdmSchema ParseSchema(XElement element)
        {
            var schemaRoot = element.Descendants(null, "Schema");

            ParseEnumTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EnumType")));
            ParseComplexTypes(schemaRoot.SelectMany(x => x.Descendants(null, "ComplexType")));
            ParseEntityTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EntityType")));
            //ParseAssociations(schemaRoot.SelectMany(x => x.Descendants(null, "Association")));
            ParseEntityContainers(schemaRoot.SelectMany(x => x.Descendants(null, "EntityContainer")));

            return new EdmSchema(this);
        }

        private void ParseEnumTypes(IEnumerable<XElement> elements)
        {
            Func<XElement, string, string> GetEnumTypeName = (x,ns) => String.Format("{0}.{1}", ns, x.Attribute("Name").Value);
            this.EnumTypes = (from e in elements select new EdmEnumType
            {
                Namespace = ParseNamespace(e),
                Name = GetEnumTypeName(e, ParseNamespace(e)),
                UnderlyingType = ParseStringAttribute(e.Attribute("UnderlyingType")),
                IsFlags = ParseBooleanAttribute(e.Attribute("IsFlags"))
            }).ToList();

            foreach (var element in elements)
            {
                var enumType = this.EnumTypes.Single(x => x.Name == GetEnumTypeName(element, x.Namespace));
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
            Func<XElement, string, string> GetComplexTypeName = (x, ns) => String.Format("{0}.{1}", ns, x.Attribute("Name").Value);
            this.ComplexTypes = (from e in elements select new EdmComplexType
            {
                Namespace = ParseNamespace(e),
                Name = GetComplexTypeName(e, ParseNamespace(e))
            }).ToList();
            
            foreach (var element in elements)
            {
                var complexType = this.ComplexTypes.Single(x => x.Name == GetComplexTypeName(element, x.Namespace));
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
                                  Namespace = ParseNamespace(e),
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
                       Namespace = r.EntityType.Namespace,
                       Name = r.EntityType.Name,
                       BaseType = String.IsNullOrEmpty(r.BaseType) ? null : results.Single(y => y.EntityType.Name == r.BaseType.Split('.').Last()).EntityType,
                       Abstract = r.EntityType.Abstract,
                       OpenType = r.EntityType.OpenType,
                       Key = r.EntityType.Key,
                       Properties = r.EntityType.Properties,
                       NavigationProperties = r.EntityType.NavigationProperties,
                   };
        }

        private void ParseEntityContainers(IEnumerable<XElement> elements)
        {
            this.EntityContainers = from e in elements
                   select new EdmEntityContainer()
                   {
                       Namespace = ParseNamespace(e),
                       Name = e.Attribute("Name").Value,
                       IsDefaulEntityContainer = ParseBooleanAttribute(e.Attribute("m", "IsDefaultEntityContainer")),
                       EntitySets = (from s in e.Descendants(null, "EntitySet")
                                     select new EdmEntitySet()
                                     {
                                         Name = s.Attribute("Name").Value,
                                         EntityType = s.Attribute("EntityType").Value,
                                     }).ToArray(),
                   };

        }

        private string ParseNamespace(XElement element)
        {
            //XNamespace xlmns = "http://schemas.microsoft.com/ado/2009/11/edm";
            return element.Ancestors(element.Name.Namespace + "Schema").Attributes("Namespace").Single().Value;
        }

        private EdmProperty ParseProperty(XElement element)
        {
            return new EdmProperty
            {
                Name = element.Attribute("Name").Value,
                Type = EdmPropertyType.Parse(element.Attribute("Type").Value, this.EntityTypes, this.ComplexTypes, this.EnumTypes),
                Nullable = ParseBooleanAttribute(element.Attribute("Nullable"), true),
                ConcurrencyMode = ParseStringAttribute(element.Attribute("ConcurrencyMode")),
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