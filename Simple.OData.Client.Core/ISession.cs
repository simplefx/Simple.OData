using System.Net;

namespace Simple.OData.Client
{
    internal interface ISession
    {
        string UrlBase { get; }
        ICredentials Credentials { get; }
        ODataProvider Provider { get; }
        IPluralizer Pluralizer { get; }
    }
}