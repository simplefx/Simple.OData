using FluentAssertions;
using Simple.OData.Client.Tests.Entities;
using Xunit;

namespace Simple.OData.Client.Tests.Core;

public class DynamicContainerTests
{
	private static ITypeCache TypeCache => TypeCaches.TypeCache("test", null);

	[Fact]
	public void ContainerName()
	{
		TypeCache.Register<Animal>();

		TypeCache.DynamicContainerName(typeof(Animal)).Should().Be("DynamicProperties");
	}

	[Fact]
	public void ExplicitContainerName()
	{
		TypeCache.Register<Animal>("Foo");

		TypeCache.DynamicContainerName(typeof(Animal)).Should().Be("Foo");
	}

	[Fact]
	public void SubTypeContainerName()
	{
		TypeCache.Register<Animal>();

		TypeCache.DynamicContainerName(typeof(Mammal)).Should().Be("DynamicProperties");
	}
}
