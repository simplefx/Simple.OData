using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests.Core;

public class ODataExpandAssociationTests
{
	[Fact]
	public void CreateExpandAssociationFromString()
	{
		var association = ODataExpandAssociation.From("Products");

		association.Name.Should().Be("Products");
	}

	[Fact]
	public void CreateExpandAssociationWithNestedAssociations()
	{
		var association = ODataExpandAssociation.From("Products/Category/Orders");

		association.Name.Should().Be("Products");
		Assert.Single(association.ExpandAssociations);
		association.ExpandAssociations.First().Name.Should().Be("Category");
		Assert.Single(association.ExpandAssociations.First().ExpandAssociations);
		association.ExpandAssociations.First().ExpandAssociations.First().Name.Should().Be("Orders");
	}

	[Fact]
	public void CreateExpandAssociationFromNullStringThrowsArgumentException()
	{
		(() => ODataExpandAssociation.From(null)).Should().ThrowExactly<ArgumentException>();
	}

	[Fact]
	public void CreateExpandAssociationFromEmptyStringThrowsArgumentException()
	{
		(() => ODataExpandAssociation.From(string.Empty)).Should().ThrowExactly<ArgumentException>();
	}

	[Fact]
	public void CloneProducesNewObjects()
	{
		var association = new ODataExpandAssociation("Products")
		{
			ExpandAssociations = { new ODataExpandAssociation("Category") }
		};

		var clonedAssociation = association.Clone();

		clonedAssociation.Should().NotBeSameAs(association);
		clonedAssociation.ExpandAssociations.First().Should().NotBeSameAs(association.ExpandAssociations.First());
	}
}
