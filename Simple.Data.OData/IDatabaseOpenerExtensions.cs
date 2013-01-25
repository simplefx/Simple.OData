using System;
using System.Net;

namespace Simple.Data.OData
{
    public static class IDatabaseOpenerExtensions
    {
        public static dynamic Open(this IDatabaseOpener opener, string url)
        {
            return opener.Open("OData", CreateSettings(url, null));
        }

        public static dynamic Open(this IDatabaseOpener opener, Uri uri)
        {
            return opener.Open("OData", CreateSettings(uri.AbsoluteUri, null));
        }

        public static dynamic Open(this IDatabaseOpener opener, ODataFeed settings)
        {
            return opener.Open("OData", CreateSettings(settings.Url, settings.Credentials));
        }

        private static object CreateSettings(string url, ICredentials credentials)
        {
            return new
                       {
                           Url = url,
                           Credentials = credentials,
                       };
        }
    }
}
