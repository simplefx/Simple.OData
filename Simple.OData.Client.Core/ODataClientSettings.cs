using System;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    public class ODataClientSettings
    {
        public string UrlBase { get; set; }
        public ICredentials Credentials { get; set; }
        public bool IncludeResourceTypeInEntryProperties { get; set; }
        public bool IgnoreResourceNotFoundException { get; set; }

        public Action<HttpRequestMessage> BeforeRequest { get; set; }
        public Action<HttpResponseMessage> AfterResponse { get; set; }

        public ODataClientSettings()
        {
        }

        public ODataClientSettings(string urlBase, ICredentials credentials = null)
        {
            this.UrlBase = urlBase;
            this.Credentials = credentials;
        }
    }
}