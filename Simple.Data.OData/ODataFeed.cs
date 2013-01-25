using System.Net;

namespace Simple.Data.OData
{
    public class ODataFeed
    {
        public string Url { get; set; }
        public ICredentials Credentials { get; set; }

        public ODataFeed()
        {
        }

        public ODataFeed(string url, ICredentials credentials = null)
        {
            this.Url = url;
            this.Credentials = credentials;
        }
    }
}