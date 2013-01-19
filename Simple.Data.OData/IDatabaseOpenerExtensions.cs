using System;

namespace Simple.Data.OData
{
    public static class IDatabaseOpenerExtensions
    {
        public static dynamic Open(this IDatabaseOpener opener, string url)
        {
            return opener.Open("OData", CreateSettings(url, null, null, null, false));
        }

        public static dynamic Open(this IDatabaseOpener opener, Uri uri)
        {
            return opener.Open("OData", CreateSettings(uri.AbsoluteUri, null, null, null, false));
        }

        public static dynamic Open(this IDatabaseOpener opener, ODataFeed settings)
        {
            return opener.Open("OData", CreateSettings(settings.Url, settings.User, settings.Password, settings.Domain, settings.IntegratedSecurity));
        }

        private static object CreateSettings(string url, string user, string password, string domain, bool integratedSecurity)
        {
            return new
                       {
                           Url = url,
                           User = user,
                           Password = password,
                           Domain = domain,
                           IntegratedSecurity = integratedSecurity,
                       };
        }
    }
}
