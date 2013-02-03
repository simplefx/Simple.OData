using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Xml.Linq;
using NExtLib.TestUtils;

#if NETFX_CORE
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
#endif

namespace Simple.NExtLib.Tests.Xml.Linq
{
    public class XElementExtensionsTests
    {
        [Fact]
        public void TestXElementWithDefaultNamespace()
        {
            var content = Properties.Resources.XmlWithDefaultNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public void TestXElementWithNoNamespace()
        {
            var content = Properties.Resources.XmlWithNoNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public void TestXElementWithPrefixedNamespace()
        {
            var content = Properties.Resources.XmlWithPrefixedNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements("c", "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element("c", "sub").Value.ShouldEqual("Foo");
            list[1].Element("c", "sub").Value.ShouldEqual("Bar");
        }
    }
}
