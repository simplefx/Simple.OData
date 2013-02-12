using System;
using System.Collections.Generic;
using System.Net;

namespace Simple.OData.Client
{
    abstract class RequestRunner
    {
        public Action<HttpWebRequest> BeforeRequest { get; set; }
        public Action<HttpWebResponse> AfterResponse { get; set; }

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
                if (this.BeforeRequest != null)
                    this.BeforeRequest(request);

                var response = (HttpWebResponse)request.GetResponse();

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
                    return Utils.StreamToString(stream);
                }
            }

            return String.Empty;
        }
    }
}
