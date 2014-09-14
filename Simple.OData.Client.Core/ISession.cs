using System.Net;

namespace Simple.OData.Client
{
    internal interface ISession
    {
        string UrlBase { get; }
        ICredentials Credentials { get; }
        ODataAdapter Adapter { get; }
        IMetadata Metadata { get; }
        IPluralizer Pluralizer { get; }
    }
}