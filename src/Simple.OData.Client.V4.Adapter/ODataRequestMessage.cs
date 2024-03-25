using Microsoft.OData;

namespace Simple.OData.Client.V4.Adapter;

internal class ODataRequestMessage : IODataRequestMessageAsync
{
	private MemoryStream _stream;
	private readonly Dictionary<string, string> _headers = [];

	public ODataRequestMessage()
	{
	}

	public string GetHeader(string headerName)
	{
		return _headers.TryGetValue(headerName, out var value) ? value : null;
	}

	public void SetHeader(string headerName, string headerValue)
	{
		_headers.Add(headerName, headerValue);
	}

	public Stream GetStream()
	{
		return _stream ??= new MemoryStream();
	}

	public Task<Stream> GetStreamAsync()
	{
		var completionSource = new TaskCompletionSource<Stream>();
		completionSource.SetResult(GetStream());
		return completionSource.Task;
	}

	public IEnumerable<KeyValuePair<string, string>> Headers => _headers;

	public Uri Url { get; set; }

	public string Method { get; set; }
}
