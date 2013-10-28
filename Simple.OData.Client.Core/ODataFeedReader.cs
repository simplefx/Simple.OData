using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class ODataFeedReader
    {
        private readonly bool _includeResourceTypeInEntryProperties;
        private readonly ISchema _schema;

        public ODataFeedReader(ISchema schema, bool includeResourceTypeInEntryProperties = false)
        {
            _schema = schema;
            _includeResourceTypeInEntryProperties = includeResourceTypeInEntryProperties;
        }

        public static EdmSchema GetSchema(Stream stream)
        {
            return GetSchema(Utils.StreamToString(stream));
        }

        public static string GetSchemaAsString(Stream stream)
        {
            return Utils.StreamToString(stream);
        }

        public static EdmSchema GetSchema(string text)
        {
            var feed = XElement.Parse(text);
            return EdmSchemaParser.ParseSchema(feed);
        }

        public IEnumerable<IDictionary<string, object>> GetData(Stream stream, bool scalarResult = false)
        {
            return GetData(Utils.StreamToString(stream), scalarResult);
        }

        public IEnumerable<IDictionary<string, object>> GetData(Stream stream, out int totalCount)
        {
            var text = Utils.StreamToString(stream);
            return GetData(text, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> GetData(string text, bool scalarResult = false)
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

        public IEnumerable<IDictionary<string, object>> GetData(string text, out int totalCount)
        {
            var feed = XElement.Parse(text);
            totalCount = GetDataCount(feed);
            return GetData(feed);
        }

        public IEnumerable<IDictionary<string, object>> GetFunctionResult(Stream stream)
        {
            var text = Utils.StreamToString(stream);
            var element = XElement.Parse(text);
            if (element.Name.LocalName == "feed")
            {
                return GetData(element);
            }
            else
            {
                Func<object, Dictionary<string, object>> ValueToResultDictionary = v =>
                    new Dictionary<string, object>() { { ODataCommand.ResultLiteral, v } };

                object value;
                try
                {
                    var collectionElements = element.Elements(null, "element");
                    if (collectionElements.Any())
                    {
                        return collectionElements.Select(x =>
                            ValueToResultDictionary(EdmTypeSerializer.Read(x).Value));
                    }
                    else
                    {
                        return new[] { ValueToResultDictionary(EdmTypeSerializer.Read(element).Value) };
                    }
                }
                catch (Exception)
                {
                    return new[] { ValueToResultDictionary(text) };
                }
            }
        }

        private IEnumerable<IDictionary<string, object>> GetData(XElement feed)
        {
            var mediaStream = feed.Element(null, "entry") != null &&
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

                var keys = GetKeys(entry);
                foreach (var kv in keys)
                {
                    if (!string.IsNullOrEmpty(kv.Key))
                        entryData.Add(kv.Key, kv.Value);
                }

                var entityElement = mediaStream ? entry : entry.Element(null, "content");
                var properties = GetProperties(entityElement).ToIDictionary();

                foreach (var property in properties)
                {
                    if (!entryData.ContainsKey(property.Key))
                        entryData.Add(property.Key, property.Value);
                }

                if (_includeResourceTypeInEntryProperties)
                {
                    var resourceType = entry.Element(null, "category").Attribute("term").Value.Split('.').Last();
                    entryData.Add(ODataCommand.ResourceTypeLiteral, resourceType);
                }

                yield return entryData;
            }
        }

        private int GetDataCount(XElement feed)
        {
            var count = feed.Elements("m", "count").SingleOrDefault();
            return count == null ? 0 : Convert.ToInt32(count.Value);
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeys(XElement element)
        {
            var content = element.Element(null, "id").Value;
            var start = content.IndexOf('(') + 1;
            var end = content.LastIndexOf(')');
            var prefix = content.Substring(0, start);
            var tableName = prefix.Substring(prefix.LastIndexOf('/') + 1);
            content = content.Substring(start, end - start);

            var table = _schema.FindBaseTable(tableName);
            return new ValueParser(table).Parse(content);
        }

        private IEnumerable<KeyValuePair<string, object>> GetProperties(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var properties = element.Element("m", "properties");

            if (properties == null) yield break;

            foreach (var property in properties.Elements())
            {
                yield return EdmTypeSerializer.Read(property);
            }
        }

        private object GetLinks(XElement element)
        {
            var feed = element.Element("m", "inline").Elements().SingleOrDefault();
            if (feed == null)
                return null;

            var linkData = GetData(feed);
            return feed.Name.LocalName == "feed" ? (object)linkData : linkData.Single();
        }
    }
}
