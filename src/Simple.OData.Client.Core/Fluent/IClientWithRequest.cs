namespace Simple.OData.Client;

public interface IClientWithRequest<T>
{
	ODataRequest GetRequest();
	IClientWithResponse<T> FromResponse(HttpResponseMessage responseMessage);

	Task<IClientWithResponse<T>> RunAsync();
	Task<IClientWithResponse<T>> RunAsync(CancellationToken cancellationToken);
}
