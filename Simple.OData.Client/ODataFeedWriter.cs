using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.NExtLib;

namespace Simple.OData.Client
{
    static class ODataFeedWriter
    {
        public static XElement CreateDataElement(string namespaceName, string collectionName, IDictionary<string, object> row)
        {
            var entry = CreateEmptyEntryWithNamespaces();

            var resourceName = GetQualifiedResourceName(namespaceName, collectionName);
            entry.Element(null, "category").SetAttributeValue("term", resourceName);
            var properties = entry.Element(null, "content").Element("m", "properties");

            foreach (var prop in row)
            {
                EdmTypeSerializer.Write(properties, prop);
            }

            return entry;
        }

        public static XElement CreateLinkElement(string link)
        {
            var entry = CreateEmptyMetadataWithNamespaces();

            entry.SetValue(link);

            return entry;
        }

        public static XElement CreateLinkElement(int contentId)
        {
            return CreateLinkElement(CreateLinkPath(contentId));
        }

        public static string CreateLinkPath(int contentId)
        {
            return "$" + contentId.ToString();
        }

        public static string CreateLinkCommand(string entryPath, string linkName)
        {
            return string.Format("{0}/$links/{1}", entryPath, linkName);
        }

        public static void AddDataLink(XElement container, string associationName, string linkedEntityName, IEnumerable<object> linkedEntityKeyValues)
        {
            var entry = XElement.Parse(Properties.Resources.DataServicesAtomEntryXml).Element(null, "link");
            var rel = entry.Attribute("rel");
            rel.SetValue(rel.Value + associationName);
            entry.SetAttributeValue("title", associationName);
            entry.SetAttributeValue("href", string.Format("{0}({1})",
                linkedEntityName,
                string.Join(",", linkedEntityKeyValues.Select(new ValueFormatter().FormatContentValue))));
            container.Add(entry);
        }

        private static XElement CreateEmptyEntryWithNamespaces()
        {
            var entry = XElement.Parse(Properties.Resources.DataServicesAtomEntryXml);
            entry.Element(null, "updated").SetValue(DateTime.UtcNow.ToIso8601String());
            entry.Element(null, "link").Remove();
            return entry;
        }

        private static XElement CreateEmptyMetadataWithNamespaces()
        {
            var entry = XElement.Parse(Properties.Resources.DataServicesMetadataEntryXml);
            return entry;
        }

        private static string GetQualifiedResourceName(string namespaceName, string collectionName)
        {
            return string.Join(".", namespaceName, collectionName);
        }
    }
}
