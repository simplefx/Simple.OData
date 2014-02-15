using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public abstract class RequestRunnerBase
    {
        public Action<HttpRequestMessage> BeforeRequest { get; set; }
        public Action<HttpResponseMessage> AfterResponse { get; set; }

        public async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequest request)
        {
            try
            {
                var clientHandler = new HttpClientHandler();

                // Perform this test to prevent failure to access Credentials/PreAuthenticate properties on SL5
                if (request.Credentials != null)
                    clientHandler.Credentials = request.Credentials;
                if (request.PreAuthenticate)
                    clientHandler.PreAuthenticate = request.PreAuthenticate;

                using (var httpClient = new HttpClient(clientHandler))
                {
                    if (request.Accept != null)
                    {
                        foreach (var accept in request.Accept)
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
                        }
                    }

                    var requestMessage = CreateRequestMessage(request);
                    if (this.BeforeRequest != null)
                        this.BeforeRequest(requestMessage);

                    var responseMessage = await httpClient.SendAsync(requestMessage);

                    if (this.AfterResponse != null)
                        this.AfterResponse(responseMessage);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        throw new WebRequestException(responseMessage.ReasonPhrase, responseMessage.StatusCode);
                    }

                    return responseMessage;
                }
            }
            catch (WebException ex)
            {
                throw WebRequestException.CreateFromWebException(ex);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is WebException)
                {
                    throw WebRequestException.CreateFromWebException(ex.InnerException as WebException);
                }
                else
                {
                    throw;
                }
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Uri);

            if (request.Content != null)
            {
                requestMessage.Content = request.Content;
            }
            return requestMessage;
        }
    }
}
