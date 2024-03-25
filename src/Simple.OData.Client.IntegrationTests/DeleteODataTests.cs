using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests;

public class DeleteODataTestsV2Atom : DeleteODataTests
{
	public DeleteODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom, 2) { }
}

public class DeleteODataTestsV2Json : DeleteODataTests
{
	public DeleteODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json, 2) { }
}

public class DeleteODataTestsV3Atom : DeleteODataTests
{
	public DeleteODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom, 3) { }
}

public class DeleteODataTestsV3Json : DeleteODataTests
{
	public DeleteODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json, 3) { }
}

public class DeleteODataTestsV4Json : DeleteODataTests
{
	public DeleteODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json, 4) { }
}

public abstract class DeleteODataTests(string serviceUri, ODataPayloadFormat payloadFormat, int version) : ODataTestBase(serviceUri, payloadFormat, version)
{
	[Fact]
	public async Task DeleteByKey()
	{
		var product = await _client
			.For("Products")
			.Set(CreateProduct(3001, "Test1"))
			.InsertEntryAsync();

		await _client
			.For("Products")
			.Key(product["ID"])
			.DeleteEntryAsync();

		product = await _client
			.For("Products")
			.Filter("Name eq 'Test1'")
			.FindEntryAsync();

		product.Should().BeNull();
	}

	[Fact]
	public async Task DeleteByFilter()
	{
		_ = await _client
			.For("Products")
			.Set(CreateProduct(3002, "Test1"))
			.InsertEntryAsync();

		await _client
			.For("Products")
			.Filter("Name eq 'Test1'")
			.DeleteEntryAsync();

		var product = await _client
			.For("Products")
			.Filter("Name eq 'Test1'")
			.FindEntryAsync();

		product.Should().BeNull();
	}

	[Fact]
	public async Task DeleteByObjectAsKey()
	{
		var product = await _client
			.For("Products")
			.Set(CreateProduct(3003, "Test1"))
			.InsertEntryAsync();

		await _client
			.For("Products")
			.Key(product)
			.DeleteEntryAsync();

		product = await _client
			.For("Products")
			.Filter("Name eq 'Test1'")
			.FindEntryAsync();

		Assert.Null(product);
	}
}
