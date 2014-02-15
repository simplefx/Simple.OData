using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;
using Simple.OData.Client.TestUtils;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class XmlAttributesAsDictionaryReadTests
    {
        [Fact]
        public void ReadWithNoNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo bar=""quux""/>"));
            xml.Attributes["bar"].ShouldEqual("quux");
        }

        [Fact]
        public void ReadWithDefaultNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo bar=""quux"" xmlns=""www.test.org""/>"));
            xml.Attributes["bar"].ShouldEqual("quux");
        }

        [Fact]
        public void ReadWithPrefixedNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo xmlns:q=""www.test.org"" q:bar=""quux""/>"));
            xml.Attributes["q:bar"].ShouldEqual("quux");
        }

        [Fact]
        public void PlainAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml.Attributes["bar"] = "quux";

            xml.ToElement().Attribute("bar").Value.ShouldEqual("quux");
        }

        [Fact]
        public void DefaultNamespaceAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo", "www.test.org");
            xml.Attributes["bar"] = "quux";

            xml.ToElement().Attribute(xml.ToElement().GetDefaultNamespace() + "bar").Value.ShouldEqual("quux");
        }

        [Fact]
        public void PrefixedNamespaceAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml.AddPrefixedNamespace("q", "www.test.org");
            xml.Attributes["q:bar"] = "quux";

            xml.ToElement().Attribute(xml.ToElement().GetNamespaceOfPrefix("q") + "bar").Value.ShouldEqual("quux");
        }
    }
}
