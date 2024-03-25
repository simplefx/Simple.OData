using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests;

public class ErrorODataTestsV2Atom : ErrorODataTests
{
	public ErrorODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom, 2) { }
}

public class ErrorODataTestsV2Json : ErrorODataTests
{
	public ErrorODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json, 2) { }
}

public class ErrorODataTestsV3Atom : ErrorODataTests
{
	public ErrorODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom, 3) { }
}

public class ErrorODataTestsV3Json : ErrorODataTests
{
	public ErrorODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json, 3) { }
}

public class ErrorODataTestsV4Json : ErrorODataTests
{
	public ErrorODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json, 4) { }
}

public abstract class ErrorODataTests(string serviceUri, ODataPayloadFormat payloadFormat, int version) : ODataTestBase(serviceUri, payloadFormat, version)
{
	[Fact]
	public async Task ErrorContent()
	{
		try
		{
			await _client
				.For("Products")
				.Filter("NonExistingProperty eq 1")
				.FindEntryAsync();

			true.Should().BeFalse("Expected exception");
		}
		catch (WebRequestException ex)
		{
			ex.Response.Should().NotBeNull();
		}
		catch (Exception)
		{
			true.Should().BeFalse("Expected WebRequestException");
		}
	}

	[Fact]
	public async Task ErrorMessage_ReasonPhrase()
	{
		try
		{
			var client = new ODataClient(CreateDefaultSettings(x =>
				x.WebRequestExceptionMessageSource = WebRequestExceptionMessageSource.ReasonPhrase));

			await client
				.For("Products")
				.Filter("NonExistingProperty eq 1")
				.FindEntryAsync();

			true.Should().BeFalse("Expected exception");
		}
		catch (WebRequestException ex)
		{
			ex.Message.Should().NotBeNull();
			ex.Message.Should().Be(ex.ReasonPhrase);
		}
	}

	[Fact]
	public async Task ErrorMessage_ResponseContent()
	{
		try
		{
			var client = new ODataClient(CreateDefaultSettings(x =>
				x.WebRequestExceptionMessageSource = WebRequestExceptionMessageSource.ResponseContent));

			await client
				.For("Products")
				.Filter("NonExistingProperty eq 1")
				.FindEntryAsync();

			true.Should().BeFalse("Expected exception");
		}
		catch (WebRequestException ex)
		{
			ex.Message.Should().NotBeNull();
			ex.Message.Should().Be(ex.Response);
		}
	}

	[Fact]
	public async Task ErrorMessage_PhraseAndContent()
	{
		try
		{
			var client = new ODataClient(CreateDefaultSettings(x =>
				x.WebRequestExceptionMessageSource = WebRequestExceptionMessageSource.Both));

			await client
				.For("Products")
				.Filter("NonExistingProperty eq 1")
				.FindEntryAsync();

			true.Should().BeFalse("Expected exception");
		}
		catch (WebRequestException ex)
		{
			ex.Message.Should().NotBeNull();
			(ex.Message.Contains(ex.ReasonPhrase) && ex.Message.Contains(ex.Response)).Should().BeTrue();
		}
	}
}
