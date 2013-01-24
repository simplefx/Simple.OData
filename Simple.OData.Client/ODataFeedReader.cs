using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Simple.NExtLib;
using Simple.NExtLib.IO;

namespace Simple.OData.Client
{
    static class ODataFeedReader
    {
        public static IEnumerable<IDictionary<string, object>> GetData(Stream stream, bool scalarResult = false)
        {
            return GetData(QuickIO.StreamToString(stream), scalarResult);
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

        public static string GetSchemaAsString(Stream stream)
        {
            return QuickIO.StreamToString(stream);
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string text, bool scalarResult = false)
        {
            if (scalarResult)
            {
                return new[] { new Dictionary<string, object>() { { ODataCommand.ResultLiteral, text } } };
            }
            else
            {
                var feed = XElement.Parse(text);
                return GetData(feed);
            }
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
            return EdmSchemaParser.ParseSchema(feed);
        }

        public static IEnumerable<IDictionary<string, object>> GetFunctionResult(Stream stream)
        {
            var text = QuickIO.StreamToString(stream);
            var element = XElement.Parse(text);
            bool scalarResult = element.Name.LocalName != "feed";
            if (scalarResult)
            {
                KeyValuePair<string, object> kv;
                try
                {
                    kv = EdmTypeSerializer.Read(element, ODataCommand.ResultLiteral);
                }
                catch (Exception)
                {
                    kv = new KeyValuePair<string, object>(ODataCommand.ResultLiteral, text);
                }
                return new[] { new Dictionary<string, object>() { { kv.Key, kv.Value } } };
            }
            else
            {
                return GetData(element);
            }
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
                yield return EdmTypeSerializer.Read(property);
            }
        }

        private static object GetLinks(XElement element)
        {
            var feed = element.Element("m", "inline").Elements().SingleOrDefault();
            if (feed == null)
                return null;

            var linkData = GetData(feed);
            return feed.Name.LocalName == "feed" ? (object)linkData : linkData.Single();
        }

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
