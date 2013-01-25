using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
                foreach (var property in properties)
                {
                    entryData.Add(property.Key, property.Value);
                }

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
    }
}
