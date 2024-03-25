using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests.Extensions;

public class TypeCacheTests
{
	private static ITypeCache TypeCache => TypeCaches.TypeCache("test", null);

	[Fact]
	public void GetDerivedTypes_BaseType()
	{
		Assert.Single(TypeCache.GetDerivedTypes(typeof(Transport)));
	}

	[Fact]
	public void GetAllProperties_BaseType()
	{
		Assert.Single(TypeCache.GetAllProperties(typeof(Transport)));
	}

	[Fact]
	public void GetAllProperties_DerivedType()
	{
		Assert.Equal(2, TypeCache.GetAllProperties(typeof(Ship)).Count());
	}

	[Fact]
	public void GetDeclaredProperties_ExcludeExplicitInterface()
	{
		Assert.Equal(5, TypeCache.GetAllProperties(typeof(Address)).Count());
	}

	[Fact]
	public void GetDeclaredProperties_BaseType()
	{
		Assert.Single(TypeCache.GetDeclaredProperties(typeof(Transport)));
	}

	[Fact]
	public void GetDeclaredProperties_DerivedType()
	{
		Assert.Single(TypeCache.GetDeclaredProperties(typeof(Ship)));
	}

	[Fact]
	public void GetNamedProperty_BaseType()
	{
		TypeCache.GetNamedProperty(typeof(Transport), "TransportID").Should().NotBeNull();
	}

	[Fact]
	public void GetNamedProperty_DerivedType()
	{
		TypeCache.GetNamedProperty(typeof(Ship), "TransportID").Should().NotBeNull();
		Assert.NotNull(TypeCache.GetNamedProperty(typeof(Ship), "ShipName"));
	}

	[Fact]
	public void GetDeclaredProperty_BaseType()
	{
		Assert.NotNull(TypeCache.GetDeclaredProperty(typeof(Transport), "TransportID"));
	}

	[Fact]
	public void GetDeclaredProperty_DerivedType()
	{
		Assert.Null(TypeCache.GetDeclaredProperty(typeof(Ship), "TransportID"));
		Assert.NotNull(TypeCache.GetDeclaredProperty(typeof(Ship), "ShipName"));
	}
}
