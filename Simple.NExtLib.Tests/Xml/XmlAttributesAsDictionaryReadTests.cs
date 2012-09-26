using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NExtLib.Unit;
using Xunit;
using System.Xml.Linq;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
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
    }
}
