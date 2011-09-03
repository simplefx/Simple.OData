using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Xml.Linq;
using NExtLib.Unit;

namespace Simple.NExtLib.Tests.Xml.Linq
{
    
    public class XElementExtensionsTests
    {
        [Fact]
        public void TestXElementWithDefaultNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithDefaultNamespace);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public void TestXElementWithNoNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithNoNamespace);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public void TestXElementWithPrefixedNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithPrefixedNamespace);
            var list = element.Elements("c", "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element("c", "sub").Value.ShouldEqual("Foo");
            list[1].Element("c", "sub").Value.ShouldEqual("Bar");
        }
    }
}
