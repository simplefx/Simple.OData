using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public abstract class RequestRunnerBase
    {
        public Action<HttpWebRequest> BeforeRequest { get; set; }
        public Action<HttpWebResponse> AfterResponse { get; set; }

        public HttpResponseMessage ExecuteRequest(HttpRequest request)
        {
            try
            {
                var clientHandler = new HttpClientHandler();
                clientHandler.Credentials = request.Credentials;
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
                    var response = httpClient.SendAsync(requestMessage).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new WebRequestException(response.ReasonPhrase, response.StatusCode);
                    }

                    return response;
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
