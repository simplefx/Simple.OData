using FluentAssertions;
using Simple.OData.Client.Extensions;
using Xunit;

namespace Simple.OData.Client.Tests;

public class TypeExtensionTests
{
	[Fact]
	public void GetAllProperties_BaseType()
	{
		Assert.Single(typeof(Transport).GetAllProperties());
	}

	[Fact]
	public void GetAllProperties_DerivedType()
	{
		Assert.Equal(2, typeof(Ship).GetAllProperties().Count());
	}

	[Fact]
	public void GetAllProperties_SkipIndexer()
	{
		Assert.Single(typeof(TypeWithIndexer).GetAllProperties());
	}

	[Fact]
	public void GetDeclaredProperties_ExcludeExplicitInterface()
	{
		Assert.Equal(5, typeof(Address).GetAllProperties().Count());
	}

	[Fact]
	public void GetDeclaredProperties_BaseType()
	{
		Assert.Single(typeof(Transport).GetDeclaredProperties());
	}

	[Fact]
	public void GetDeclaredProperties_DerivedType()
	{
		Assert.Single(typeof(Ship).GetDeclaredProperties());
	}

	[Fact]
	public void GetNamedProperty_BaseType()
	{
		typeof(Transport).GetNamedProperty("TransportID").Should().NotBeNull();
	}

	[Fact]
	public void GetNamedProperty_DerivedType()
	{
		typeof(Ship).GetNamedProperty("TransportID").Should().NotBeNull();
		typeof(Ship).GetNamedProperty("ShipName").Should().NotBeNull();
	}

	[Fact]
	public void GetDeclaredProperty_BaseType()
	{
		typeof(Transport).GetDeclaredProperty("TransportID").Should().NotBeNull();
	}

	[Fact]
	public void GetDeclaredProperty_DerivedType()
	{
		typeof(Ship).GetDeclaredProperty("TransportID").Should().BeNull();
		typeof(Ship).GetDeclaredProperty("ShipName").Should().NotBeNull();
	}
}
