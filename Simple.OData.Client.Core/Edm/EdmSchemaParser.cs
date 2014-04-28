using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    static class EdmSchemaParser
    {
        public static EdmSchema ParseSchema(XElement element)
        {
            var schemaRoot = element.Descendants(null, "Schema");

            var typesNamespace = schemaRoot
                .Where(x => x.Descendants(null, "EntityType").Any()).FirstOrDefault().Attribute("Namespace").Value;
            var containersNamespace = schemaRoot
                .Where(x => x.Descendants(null, "EntityContainer").Any()).FirstOrDefault().Attribute("Namespace").Value;

            var complexTypes = ParseComplexTypes(schemaRoot.SelectMany(x => x.Descendants(null, "ComplexType")), typesNamespace);
            var ccc = complexTypes.ToList();
            var entityTypes = ParseEntityTypes(schemaRoot.SelectMany(x => x.Descendants(null, "EntityType")), complexTypes, typesNamespace);
            var associations = ParseAssociations(schemaRoot.SelectMany(x => x.Descendants(null, "Association")), complexTypes, entityTypes);
            var entityContainers = ParseEntityContainers(schemaRoot.SelectMany(x => x.Descendants(null, "EntityContainer")), complexTypes, entityTypes);

            return new EdmSchema(typesNamespace, containersNamespace, entityTypes, complexTypes, associations, entityContainers);
        }

        private static IEnumerable<EdmComplexType> ParseComplexTypes(
            IEnumerable<XElement> elements,
            string typesNamespace)
        {
            Func<XElement, string> GetComplexTypeName = x => String.Format("{0}.{1}", typesNamespace, x.Attribute("Name").Value);
            var complexTypes = (from e in elements select new EdmComplexType { Name = GetComplexTypeName(e) }).ToList();
            foreach (var element in elements)
            {
                var complexType = complexTypes.Single(x => x.Name == GetComplexTypeName(element));
                complexType.Properties = (from p in element.Descendants(null, "Property")
                                          select ParseProperty(p, complexTypes, new EdmEntityType[] { })).ToArray();
            }
            return complexTypes;
        }

        private static IEnumerable<EdmEntityType> ParseEntityTypes(
            IEnumerable<XElement> elements,
            IEnumerable<EdmComplexType> complexTypes,
            string typesNamespace)
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
                                                select ParseProperty(p, complexTypes, new EdmEntityType[] { })).ToArray(),
                                  NavigationProperties = (from p in e.Descendants(null, "NavigationProperty")
                                                select ParseNavigationProperty(p)).ToArray(),
                              },
                              BaseType = ParseStringAttribute(e.Attribute("BaseType")),
                          };
            return from r in results
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

        private static IEnumerable<EdmAssociation> ParseAssociations(
            IEnumerable<XElement> elements,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmEntityType> entityTypes)
        {
            return from e in elements
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

        private static IEnumerable<EdmEntityContainer> ParseEntityContainers(
            IEnumerable<XElement> elements,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmEntityType> entityTypes)
        {
            return from e in elements
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
                                              ReturnType = ParseType(s.Attribute("ReturnType"), complexTypes, entityTypes),
                                              EntitySet = ParseStringAttribute(s.Attribute("EntitySet")),
                                              Parameters = (from p in s.Descendants(null, "Parameter")
                                                            select new EdmParameter()
                                                            {
                                                                Name = p.Attribute("Name").Value,
                                                                Type = EdmPropertyType.Parse(p.Attribute("Type").Value, complexTypes, entityTypes),
                                                            }).ToArray(),
                                          }).ToArray(),
                   };

        }

        private static EdmProperty ParseProperty(
            XElement element,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmEntityType> entityTypes)
        {
            return new EdmProperty
            {
                Name = element.Attribute("Name").Value,
                Type = EdmPropertyType.Parse(element.Attribute("Type").Value, complexTypes, entityTypes),
                Nullable = ParseBooleanAttribute(element.Attribute("Nullable"), true),
            };
        }

        private static EdmNavigationProperty ParseNavigationProperty(
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

        private static EdmPropertyType ParseType(
            XAttribute attribute,
            IEnumerable<EdmComplexType> complexTypes,
            IEnumerable<EdmEntityType> entityTypes)
        {
            if (attribute == null || attribute.Value == null)
                return null;

            var attritbuteValue = ParseStringAttribute(attribute);
            return EdmPropertyType.Parse(attritbuteValue, complexTypes, entityTypes);
        }

        private static EdmKey ParseKey(XElement element)
        {
            return new EdmKey()
            {
                Properties = (from p in element.Descendants(null, "PropertyRef")
                              select p.Attribute("Name").Value).ToArray()
            };
        }

        private static bool ParseBooleanAttribute(XAttribute attribute, bool @default = false)
        {
            bool result = @default;
            if (attribute != null)
            {
                Boolean.TryParse(attribute.Value, out result);
            }
            return result;
        }

        private static string ParseStringAttribute(XAttribute attribute, string @default = null)
        {
            return attribute == null ? @default : attribute.Value;
        }
    }
}