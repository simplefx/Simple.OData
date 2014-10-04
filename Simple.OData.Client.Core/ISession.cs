using System.Net;

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
    }
}