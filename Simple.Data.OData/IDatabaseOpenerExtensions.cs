using System;

namespace Simple.Data.OData
{
    public static class IDatabaseOpenerExtensions
    {
        public static dynamic Open(this IDatabaseOpener opener, string url)
        {
            return opener.Open("OData", new { Url = url });
        }

        public static dynamic Open(this IDatabaseOpener opener, Uri uri)
        {
            return opener.Open("OData", new { Url = uri });
        }
    }
}
