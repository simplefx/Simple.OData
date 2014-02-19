using System;
using System.Net;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public abstract class RequestRunnerBase
    {
        public Action<HttpWebRequest> BeforeRequest { get; set; }
        public Action<HttpWebResponse> AfterResponse { get; set; }

        public async Task<HttpWebResponse> ExecuteRequestAsync(HttpWebRequest request)
        {
            try
            {
                if (this.BeforeRequest != null)
                    this.BeforeRequest(request);

                var response = (HttpWebResponse)(await request.GetResponseAsync());

                if (this.AfterResponse != null)
                    this.AfterResponse(response);

                return response;
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
