using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure
{
    using System.Net;
    using NExtLib.IO;

    public interface IRequestRunner
    {
        string Request(HttpWebRequest request);
        HttpWebResponse TryRequest(HttpWebRequest request);
    }

    public class RequestRunner : IRequestRunner
    {
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
