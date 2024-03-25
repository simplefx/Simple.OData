using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests;

// Not supported by OData V2
//public class MediaODataTestsV2Atom : MediaODataTests
//{
//    public MediaODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom, 2) { }
//}

//public class MediaODataTestsV2Json : MediaODataTests
//{
//    public MediaODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json, 2) { }
//}

public class MediaODataTestsV3Atom : MediaODataTests
{
	public MediaODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom, 3) { }
}

public class MediaODataTestsV3Json : MediaODataTests
{
	public MediaODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json, 3) { }
}

public class MediaODataTestsV4Json : MediaODataTests
{
	public MediaODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json, 4) { }
}

public abstract class MediaODataTests(string serviceUri, ODataPayloadFormat payloadFormat, int version) : ODataTestBase(serviceUri, payloadFormat, version)
{
	private class PersonDetail
	{
		public string Photo { get; set; }
	}

	[Fact]
	public async Task GetMediaStream()
	{
		var ad = await _client
			.For("Advertisements")
			.FindEntryAsync();
		var id = ad["ID"];
		var stream = await _client
			.For("Advertisements")
			.Key(id)
			.Media()
			.GetStreamAsync();
		var text = Utils.StreamToString(stream);
		text.Should().Contain("stream data");
	}

	[Fact]
	public async Task GetNamedMediaStream()
	{
		var stream = await _client
			.For("Persons")
			.Key(1)
			.NavigateTo("PersonDetail")
			.Media("Photo")
			.GetStreamAsync();
		var text = Utils.StreamToString(stream);
		text.Should().Contain("named stream data");
	}

	[Fact]
	public async Task GetTypedNamedMediaStream()
	{
		var text = await _client
			.For("Persons")
			.Key(1)
			.NavigateTo<PersonDetail>()
			.Media(x => x.Photo)
			.GetStreamAsStringAsync();
		text.Should().Contain("named stream data");
	}

	[Fact]
	public async Task FindEntryWithEntityMedia()
	{
		var ad = await _client
			.For("Advertisements")
			.FindEntryAsync();
		var id = ad["ID"];

		ad = await _client
			.For("Advertisements")
			.WithMedia("Media")
			.Key(id)
			.FindEntryAsync();
		ad["Media"].Should().NotBeNull();
		var text = Utils.StreamToString(ad["Media"] as Stream);
		text.Should().Contain("stream data");
	}

	[Fact]
	public async Task FindEntryWithNamedMedia()
	{
		var person = await _client
			.For("Persons")
			.Key(1)
			.NavigateTo("PersonDetail")
			.WithMedia("Photo")
			.FindEntryAsync();
		person["Photo"].Should().NotBeNull();
		var text = Utils.StreamToString(person["Photo"] as Stream);
		text.Should().Contain("named stream data");
	}

	[Fact]
	public async Task SetMediaStream()
	{
		var ad = await _client
			.For("Advertisements")
			.FindEntryAsync();
		var id = ad["ID"];
		var stream = Utils.StringToStream("Updated stream data");
		await _client
			.For("Advertisements")
			.Key(id)
			.Media()
			.SetStreamAsync(stream, "text/plain", false);
		stream = await _client
			.For("Advertisements")
			.Key(id)
			.Media()
			.GetStreamAsync();
		var text = Utils.StreamToString(stream);
		text.Should().Be("Updated stream data");
	}

	[Fact]
	public async Task SetNamedMediaStream()
	{
		var stream = Utils.StringToStream("Updated named stream data");
		await _client
			.For("Persons")
			.Key(1)
			.NavigateTo("PersonDetail")
			.Media("Photo")
			.SetStreamAsync(stream, "text/plain", false);
		stream = await _client
			.For("Persons")
			.Key(1)
			.NavigateTo("PersonDetail")
			.Media("Photo")
			.GetStreamAsync();
		var text = Utils.StreamToString(stream);
		text.Should().Be("Updated named stream data");
	}
}
