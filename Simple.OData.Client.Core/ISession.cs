using System;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    public interface ISession
    {
        string UrlBase { get; }
        ICredentials Credentials { get; }
        ODataPayloadFormat PayloadFormat { get; }
        IODataAdapter Adapter { get; }
        IMetadata Metadata { get; }
        IPluralizer Pluralizer { get; }

        Action<HttpClientHandler> OnApplyClientHandler { get; set; }
        Action<HttpRequestMessage> BeforeRequest { get; set; }
        Action<HttpResponseMessage> AfterResponse { get; set; }
    }
}