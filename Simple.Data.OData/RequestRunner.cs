using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData
{
    using System.Net;
    using NExtLib.IO;

    public abstract class RequestRunner
    {
        protected RequestBuilder _requestBuilder;

        public RequestRunner(RequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
        }

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
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw TableServiceException.CreateFromWebException(ex);
            }
        }

        public abstract IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, bool setTotalCount, out int totalCount);
        public abstract IDictionary<string, object> InsertEntry(bool resultRequired);
        public abstract int UpdateEntry();
        public abstract int DeleteEntry();

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
