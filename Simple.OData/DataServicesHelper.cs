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
        public static IEnumerable<IDictionary<string, object>> GetData(Stream stream, bool scalarResult = false)
        {
            var text = QuickIO.StreamToString(stream);
            if (scalarResult)
                return new[] { new Dictionary<string, object>() { { "$result", text } } };
            else
                return GetData(text);
        }

        public static IEnumerable<IDictionary<string, object>> GetData(Stream stream, out int totalCount)
        {
            var text = QuickIO.StreamToString(stream);
            return GetData(text, out totalCount);
        }

        public static EdmSchema GetSchema(Stream stream)
        {
            return GetSchema(QuickIO.StreamToString(stream));
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string text)
        {
            var feed = XElement.Parse(text);
            return GetData(feed);
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string text, out int totalCount)
        {
            var feed = XElement.Parse(text);
            totalCount = GetDataCount(feed);
            return GetData(feed);
        }

        public static EdmSchema GetSchema(string text)
        {
            var feed = XElement.Parse(text);
            return ParseSchema(feed);
        }

        private static IEnumerable<IDictionary<string, object>> GetData(XElement feed)
        {
            bool mediaStream = feed.Element(null, "entry") != null &&
                               feed.Element(null, "entry").Descendants(null, "link").Attributes("rel").Any(
                                   x => x.Value == "edit-media");

            var entryElements = feed.Name.LocalName == "feed"
                              ? feed.Elements(null, "entry")
                              : new[] { feed };

            foreach (var entry in entryElements)
            {
                var entryData = new Dictionary<string, object>();

                var linkElements = entry.Elements(null, "link").Where(x => x.Descendants("m", "inline").Any());
                foreach (var linkElement in linkElements)
                {
                    var linkData = GetLinks(linkElement);
                    entryData.Add(linkElement.Attribute("title").Value, linkData);
                }

                var entityElement = mediaStream ? entry : entry.Element(null, "content");
                var properties = GetProperties(entityElement).ToIDictionary();
                properties.ToList().ForEach(x => entryData.Add(x.Key, x.Value));

                yield return entryData;
            }
        }

        private static int GetDataCount(XElement feed)
        {
            var count = feed.Elements("m", "count").SingleOrDefault();
            return count == null ? 0 : Convert.ToInt32(count.Value);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetProperties(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var properties = element.Element("m", "properties");

            if (properties == null) yield break;

            foreach (var property in properties.Elements())
            {
                yield return EdmHelper.Read(property);
            }
        }

        private static object GetLinks(XElement element)
        {
            var feed = element.Element("m", "inline").Elements().Single();
            var linkData = GetData(feed);
            return feed.Name.LocalName == "feed" ? (object)linkData : linkData.Single();
        }

        private static EdmSchema ParseSchema(XElement element)
        {
            var complexTypes = ParseComplexTypes(new EdmComplexType[] { },
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "ComplexType")));
            var entityTypes = ParseEntityTypes(complexTypes,
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "EntityType")));
            var associations = ParseAssociations(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "Association")));
            var entityContainers = ParseEntityContainers(
                element.Descendants(null, "Schema").SelectMany(x => x.Descendants(null, "EntityContainer")));
            return new EdmSchema(entityTypes, complexTypes, associations, entityContainers);
        }

        private static IEnumerable<EdmComplexType> ParseComplexTypes(IEnumerable<EdmComplexType> complexTypes, IEnumerable<XElement> elements)
        {
            return from e in elements
                   select new EdmComplexType()
                   {
                       Name = e.Attribute("Name").Value,
                       Properties = (from p in e.Descendants(null, "Property")
                                     select ParseProperty(p, complexTypes)).ToArray(),
                   };
        }

        private static IEnumerable<EdmEntityType> ParseEntityTypes(IEnumerable<EdmComplexType> complexTypes, IEnumerable<XElement> elements)
        {
            return from e in elements
                   select new EdmEntityType()
                              {
                                  Name = e.Attribute("Name").Value,
                                  Properties = (from p in e.Descendants(null, "Property")
                                                select ParseProperty(p, complexTypes)).ToArray(),
                                  Key = (from k in e.Descendants(null, "Key")
                                         select ParseKey(k)).Single(),
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
                                                         ReturnType = s.Attribute("ReturnType").Value,
                                                         EntitySet = s.Attribute("EntitySet").Value,
                                                     }).ToArray(),
                              };

        }

        private static EdmProperty ParseProperty(XElement element, IEnumerable<EdmComplexType> complexTypes)
        {
            return new EdmProperty
                       {
                           Name = element.Attribute("Name").Value,
                           Type = EdmPropertyType.Parse(element.Attribute("Type").Value, complexTypes),
                           Nullable = bool.Parse(element.Attribute("Nullable").Value),
                       };
        }

        private static EdmKey ParseKey(XElement element)
        {
            return new EdmKey()
                       {
                           Properties = (from p in element.Descendants(null, "PropertyRef")
                                         select p.Attribute("Name").Value).ToArray()
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
