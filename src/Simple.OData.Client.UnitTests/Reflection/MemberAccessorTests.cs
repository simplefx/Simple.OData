using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests.Reflection;

public class MemberAccessorTests
{
	private class TestClass
	{
		public string instanceField;
		public static string staticField = "staticFieldValue";
		public static string staticFieldToSet = "staticFieldToSetValue";

		public string InstanceProprety { get; set; }
		public static string StaticProperty { get; } = "staticPropertyValue";
		public static string StaticPropertyToSet { get; set; } = "staticPropertyToSetValue";
	}

	[Fact]
	public void ShouldGetInstancePropertyValue()
	{
		var instance = new TestClass
		{
			InstanceProprety = "instancePropertyValue"
		};

		MemberAccessor.GetValue<string>(instance, nameof(TestClass.InstanceProprety)).Should().Be(instance.InstanceProprety);
	}

	[Fact]
	public void ShouldGetInstanceFieldValue()
	{
		var instance = new TestClass
		{
			instanceField = "instanceFieldValue"
		};

		MemberAccessor.GetValue<string>(instance, nameof(TestClass.instanceField)).Should().Be(instance.instanceField);
	}

	[Fact]
	public void ShouldGetStaticPropertyValue()
	{
		MemberAccessor.GetValue<string>(null, typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty))).Should().Be(TestClass.StaticProperty);
	}

	[Fact]
	public void ShouldGetStaticFieldValue()
	{
		MemberAccessor.GetValue<string>(null, typeof(TestClass).GetField(nameof(TestClass.staticField))).Should().Be(TestClass.staticField);
	}






	[Fact]
	public void ShouldSetInstancePropertyValue()
	{
		var instance = new TestClass
		{
			InstanceProprety = "instancePropertyValue"
		};

		MemberAccessor.SetValue(instance, nameof(TestClass.InstanceProprety), "test");

		instance.InstanceProprety.Should().Be("test");
	}

	[Fact]
	public void ShouldSetInstanceFieldValue()
	{
		var instance = new TestClass
		{
			instanceField = "instanceFieldValue"
		};

		MemberAccessor.SetValue(instance, nameof(TestClass.instanceField), "test");

		instance.instanceField.Should().Be("test");
	}

	[Fact]
	public void ShouldSetStaticPropertyValue()
	{
		MemberAccessor.SetValue(null, typeof(TestClass).GetProperty(nameof(TestClass.StaticPropertyToSet)), "test");

		TestClass.StaticPropertyToSet.Should().Be("test");
	}

	[Fact]
	public void ShouldSetStaticFieldValue()
	{
		MemberAccessor.SetValue(null, typeof(TestClass).GetField(nameof(TestClass.staticFieldToSet)), "test");

		TestClass.staticFieldToSet.Should().Be("test");
	}
}

