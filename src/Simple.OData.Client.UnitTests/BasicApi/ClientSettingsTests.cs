using FluentAssertions;
using Xunit;

namespace Simple.OData.Client.Tests.BasicApi;

public class ClientSettingsTests : TestBase
{
	[Fact]
	public async Task UpdateRequestHeadersForXCsrfTokenRequests()
	{
		// Make sure the default doesn't contain any headers
		var concreteClient = _client as ODataClient;
		concreteClient.Session.Settings.BeforeRequest.Should().BeNull();

		// Add some headers - note this will simply set up the request action
		// to lazily add them to the request.
		concreteClient.UpdateRequestHeaders(new Dictionary<string, IEnumerable<string>>
			{
				{"x-csrf-token", new List<string> {"fetch"}}
			});
		concreteClient.Session.Settings.BeforeRequest.Should().NotBeNull();

		// Make sure we can still execute a request
		await concreteClient.GetMetadataDocumentAsync();
	}
}
