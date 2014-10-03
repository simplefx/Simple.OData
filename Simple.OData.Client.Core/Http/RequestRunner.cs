using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class RequestRunner
    {
        private readonly ISession _session;

        public Action<HttpRequestMessage> BeforeRequest { get; set; }
        public Action<HttpResponseMessage> AfterResponse { get; set; }

        public RequestRunner(ISession session)
        {
            _session = session;
        }

        public async Task<HttpResponseMessage> ExecuteRequestAsync(ODataRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var clientHandler = new HttpClientHandler();

                // Perform this test to prevent failure to access Credentials/PreAuthenticate properties on SL5
                if (request.Credentials != null)
                {
                    clientHandler.Credentials = request.Credentials;
                    if (clientHandler.SupportsPreAuthenticate())
                        clientHandler.PreAuthenticate = true;
                }

                using (var httpClient = new HttpClient(clientHandler))
                {
                    if (request.Accept != null)
                    {
                        foreach (var accept in request.Accept)
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
                        }
                    }

                    if (request.CheckOptimisticConcurrency &&
                        (request.Method == RestVerbs.Put ||
                        request.Method == RestVerbs.Patch ||
                        request.Method == RestVerbs.Delete))
                    {
                        httpClient.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                    }

                    if (this.BeforeRequest != null)
                        this.BeforeRequest(request.RequestMessage);

                    foreach (var header in request.Headers)
                    {
                        request.RequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    var responseMessage = await httpClient.SendAsync(request.RequestMessage, cancellationToken);

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
    }
}
