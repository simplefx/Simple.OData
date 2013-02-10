using System;
using System.Net;

namespace Simple.Data.OData
{
    public static class IDatabaseOpenerExtensions
    {
        public static dynamic Open(this IDatabaseOpener opener, string url)
        {
            return opener.Open("OData", CreateSettings(url, null, false, false, null, null));
        }

        public static dynamic Open(this IDatabaseOpener opener, Uri uri)
        {
            return opener.Open("OData", CreateSettings(uri.AbsoluteUri, null, false, false, null, null));
        }

        public static dynamic Open(this IDatabaseOpener opener, ODataFeed settings)
        {
            return opener.Open("OData", CreateSettings(
                settings.Url, 
                settings.Credentials, 
                settings.IncludeResourceTypeInEntryProperties,
                settings.IgnoreResourceNotFoundException,
                settings.BeforeRequest,
                settings.AfterResponse));
        }

        private static object CreateSettings(
            string url, 
            ICredentials credentials,
            bool includeResourceTypeInEntryProperties,
            bool ignoreResourceNotFoundException,
            Action<HttpWebRequest> beforeRequest,
            Action<HttpWebResponse> afterResponse)
        {
            return new
                       {
                           Url = url,
                           Credentials = credentials,
                           IncludeResourceTypeInEntryProperties = includeResourceTypeInEntryProperties,
                           IgnoreResourceNotFoundException = ignoreResourceNotFoundException,
                           BeforeRequest = beforeRequest,
                           AfterResponse = afterResponse,
                       };
        }
    }
}
