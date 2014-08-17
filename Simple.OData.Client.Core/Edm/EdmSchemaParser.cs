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
        public IEnumerable<EdmEntityContainer> EntityContainers { get; private set; }

        public EdmSchemaParser()
        {
            this.EntityTypes = new List<EdmEntityType>();
            this.ComplexTypes = new List<EdmComplexType>();
            this.EntityContainers = new List<EdmEntityContainer>();
        }

        public EdmSchema ParseSchema(XElement element)
        {
            var schemaRoot = element.Descendants(null, "Schema");

            //ParseEnumTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EnumType")));
            ParseComplexTypes(schemaRoot.SelectMany(x => x.Descendants(null, "ComplexType")));
            ParseEntityTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EntityType")));
            ParseEntityContainers(schemaRoot.SelectMany(x => x.Descendants(null, "EntityContainer")));

            return new EdmSchema(this);
        }

        private void ParseComplexTypes(IEnumerable<XElement> elements)
        {
            this.ComplexTypes = (from e in elements
                                 select new EdmComplexType
            {
                Namespace = ParseNamespace(e),
                Name = e.Attribute("Name").Value
            }).ToList();
            
            foreach (var element in elements)
            {
                var complexType = this.ComplexTypes.Single(x => x.Name == element.Attribute("Name").Value);
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
                                  //Key = (from k in e.Descendants(null, "Key")
                                  //       select ParseKey(k)).SingleOrDefault(),
                                  Properties = (from p in e.Descendants(null, "Property")
                                                select ParseProperty(p)).ToArray(),
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
                       //Key = r.EntityType.Key,
                       Properties = r.EntityType.Properties,
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
                Type = EdmPropertyType.Parse(element.Attribute("Type").Value, this.EntityTypes, this.ComplexTypes),
                Nullable = ParseBooleanAttribute(element.Attribute("Nullable"), true),
                ConcurrencyMode = ParseStringAttribute(element.Attribute("ConcurrencyMode")),
            };
        }

        private EdmPropertyType ParseType(XAttribute attribute)
        {
            if (attribute == null || attribute.Value == null)
                return null;

            var attritbuteValue = ParseStringAttribute(attribute);
            return EdmPropertyType.Parse(attritbuteValue, this.EntityTypes, this.ComplexTypes);
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