using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Simple.NExtLib;
using Simple.NExtLib.Xml.Syndication;
using Simple.NExtLib.IO;
using Simple.OData.Edm;

namespace Simple.OData
{
    public static class DataServicesHelper
    {
        public static IEnumerable<IDictionary<string, object>> GetData(Stream stream)
        {
            return GetData(QuickIO.StreamToString(stream));
        }

        public static EdmSchema GetSchema(Stream stream)
        {
            return GetSchema(QuickIO.StreamToString(stream));
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string text)
        {
            var feed = XElement.Parse(text);

            string elementName =
                feed.Element(null, "entry") != null && feed.Element(null, "entry").Descendants(null, "link").Attributes("rel").Any(x => x.Value == "edit-media")
                    ? "entry"
                    : "content";

            return feed.Descendants(null, elementName).Select(content => GetData(content).ToIDictionary());
        }

        public static IEnumerable<KeyValuePair<string, object>> GetData(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var properties = element.Element("m", "properties");

            if (properties == null) yield break;

            foreach (var property in properties.Elements())
            {
                yield return EdmHelper.Read(property);
            }
        }

        public static EdmSchema GetSchema(string text)
        {
            var feed = XElement.Parse(text);
            return ParseSchema(feed);
        }

        private static EdmSchema ParseSchema(XElement element)
        {
            var entityTypes = ParseEntityTypes(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "EntityType")));
            var complexTypes = ParseComplexTypes(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "ComplexType")));
            var associations = ParseAssociations(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "Association")));
            var entityContainers = ParseEntityContainers(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "EntityContainer")));
            return new EdmSchema(entityTypes, complexTypes, associations, entityContainers);
        }

        private static IEnumerable<EdmEntityType> ParseEntityTypes(IEnumerable<XElement> elements)
        {
            return from e in elements
                   select new EdmEntityType()
                              {
                                  Name = e.Attribute("Name").Value,
                                  Properties = (from p in e.Descendants(null, "Property")
                                        select new EdmProperty()
                                            {
                                                Name = p.Attribute("Name").Value,
                                                Type = EdmType.Parse(p.Attribute("Type").Value),
                                                Nullable = bool.Parse(p.Attribute("Nullable").Value),
                                            }).ToArray(),
                                  Key = (from k in e.Descendants(null, "Key")
                                        select new EdmKey()
                                            {
                                                Properties = (from p in k.Descendants(null, "PropertyRef")
                                                            select p.Attribute("Name").Value).ToArray()
                                            }).Single(),
                              };
        }

        private static IEnumerable<EdmComplexType> ParseComplexTypes(IEnumerable<XElement> elements)
        {
            return from e in elements
                   select new EdmComplexType()
                   {
                       Name = e.Attribute("Name").Value,
                       Properties = (from p in e.Descendants(null, "Property")
                                     select new EdmProperty()
                                     {
                                         Name = p.Attribute("Name").Value,
                                         Type = EdmType.Parse(p.Attribute("Type").Value),
                                         Nullable = bool.Parse(p.Attribute("Nullable").Value),
                                     }).ToArray(),
                   };
        }

        private static IEnumerable<EdmAssociation> ParseAssociations(IEnumerable<XElement> elements)
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

        private static IEnumerable<EdmEntityContainer> ParseEntityContainers(IEnumerable<XElement> elements)
        {
            return from e in elements
                   select new EdmEntityContainer()
                              {
                                  Name = e.Attribute("Name").Value,
                                  IsDefaulEntityContainer = bool.Parse(e.Attribute("m", "IsDefaultEntityContainer").Value),
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
                                            }).ToArray(),
                                  FunctionImports = null,
                              };

        }

        public static XElement CreateDataElement(IDictionary<string, object> row)
        {
            var entry = CreateEmptyEntryWithNamespaces();

            var properties = entry.Element(null, "content").Element("m", "properties");

            foreach (var prop in row)
            {
                EdmHelper.Write(properties, prop);
            }

            return entry;
        }

        private static XElement CreateEmptyEntryWithNamespaces()
        {
            var entry = XElement.Parse(Properties.Resources.DataServicesAtomEntryXml);
            entry.Element(null, "updated").SetValue(DateTime.UtcNow.ToIso8601String());
            return entry;
        }
    }
}
