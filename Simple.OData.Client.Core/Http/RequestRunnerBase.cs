using System;
using System.IO;
using System.Net;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public abstract class RequestRunnerBase
    {
        public Action<HttpWebRequest> BeforeRequest { get; set; }
        public Action<HttpWebResponse> AfterResponse { get; set; }

        public HttpWebResponse ExecuteRequest(HttpRequest request)
        {
            try
            {
                var webRequest = CreateWebRequest(request);
                
                if (this.BeforeRequest != null)
                    this.BeforeRequest(webRequest);

                var webResponse = (HttpWebResponse)webRequest.GetResponse();

                if (this.AfterResponse != null)
                    this.AfterResponse(webResponse);

                return webResponse;
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

        private HttpWebRequest CreateWebRequest(HttpRequest request)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(request.Uri);
            webRequest.Method = request.Method;
            webRequest.ContentType = request.ContentType;
            if (!string.IsNullOrEmpty(request.Content))
                webRequest.SetContent(request.Content);
            webRequest.Accept = request.Accept;

            webRequest.Credentials = request.Credentials;
#if NET40
            if (webRequest.Credentials != null)
            {
                webRequest.PreAuthenticate = request.PreAuthenticate;
                webRequest.KeepAlive = request.KeepAlive;
            }
#endif
            return webRequest;
        }
    }
}
