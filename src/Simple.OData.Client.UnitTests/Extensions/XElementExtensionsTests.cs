using System.Xml.Linq;
using FluentAssertions;
using Simple.OData.Client.Extensions;
using Simple.OData.Client.UnitTests.Properties;
using Xunit;

namespace Simple.OData.Client.Tests.Extensions;

public class XElementExtensionsTests
{
	[Fact]
	public void TestXElementWithDefaultNamespace()
	{
		var content = XmlSamples.XmlWithDefaultNamespace;
		var element = XElement.Parse(content);
		var list = element.Elements(null, "child").ToList();
		Assert.Equal(2, list.Count);
		list[0].Element(null, "sub").Value.Should().Be("Foo");
		list[1].Element(null, "sub").Value.Should().Be("Bar");
	}

	[Fact]
	public void TestXElementWithNoNamespace()
	{
		var content = XmlSamples.XmlWithNoNamespace;
		var element = XElement.Parse(content);
		var list = element.Elements(null, "child").ToList();
		Assert.Equal(2, list.Count);
		Assert.Equal("Foo", list[0].Element(null, "sub").Value);
		Assert.Equal("Bar", list[1].Element(null, "sub").Value);
	}

	[Fact]
	public void TestXElementWithPrefixedNamespace()
	{
		var content = XmlSamples.XmlWithPrefixedNamespace;
		var element = XElement.Parse(content);
		var list = element.Elements("c", "child").ToList();
		Assert.Equal(2, list.Count);
		Assert.Equal("Foo", list[0].Element("c", "sub").Value);
		Assert.Equal("Bar", list[1].Element("c", "sub").Value);
	}
}
