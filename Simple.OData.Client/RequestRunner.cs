using System;
using System.Collections.Generic;
using System.Net;
using Simple.NExtLib.IO;

namespace Simple.OData.Client
{
    abstract class RequestRunner
    {
        public Action<HttpWebRequest> RequestInterceptor { get; set; }
        public Action<HttpWebResponse> ResponseInterceptor { get; set; }

        public string Request(HttpWebRequest request)
        {
            using (var response = TryRequest(request))
            {
                return TryGetResponseBody(response);
            }
        }

        public HttpWebResponse TryRequest(HttpWebRequest request)
        {
            try
            {
                if (this.RequestInterceptor != null)
                    this.RequestInterceptor(request);

                var response = (HttpWebResponse)request.GetResponse();

                if (this.ResponseInterceptor != null)
                    this.ResponseInterceptor(response);

                return response;
            }
            catch (WebException ex)
            {
                throw WebRequestException.CreateFromWebException(ex);
            }
        }

        public abstract IEnumerable<IDictionary<string, object>> FindEntries(HttpCommand command, bool scalarResult, bool setTotalCount, out int totalCount);
        public abstract IDictionary<string, object> GetEntry(HttpCommand command);
        public abstract IDictionary<string, object> InsertEntry(HttpCommand command, bool resultRequired);
        public abstract int UpdateEntry(HttpCommand command);
        public abstract int DeleteEntry(HttpCommand command);
        public abstract IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(HttpCommand command);

        private static string TryGetResponseBody(HttpWebResponse response)
        {
            if (response != null)
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    return QuickIO.StreamToString(stream);
                }
            }

            return String.Empty;
        }
    }
}
