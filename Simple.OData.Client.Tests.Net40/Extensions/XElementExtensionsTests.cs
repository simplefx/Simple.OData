using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;
using Simple.OData.Client.TestUtils;
using Xunit;

#if NETFX_CORE
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
#endif

namespace Simple.OData.Client.Tests
{
    public class XElementExtensionsTests
    {
        [Fact]
        public async Task TestXElementWithDefaultNamespace()
        {
            var content = Properties.XmlSamples.XmlWithDefaultNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public async Task TestXElementWithNoNamespace()
        {
            var content = Properties.XmlSamples.XmlWithNoNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Fact]
        public async Task TestXElementWithPrefixedNamespace()
        {
            var content = Properties.XmlSamples.XmlWithPrefixedNamespace;
            var element = XElement.Parse(content);
            var list = element.Elements("c", "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element("c", "sub").Value.ShouldEqual("Foo");
            list[1].Element("c", "sub").Value.ShouldEqual("Bar");
        }
    }
}
