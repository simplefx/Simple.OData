using System;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    public interface ISession
    {
        ODataClientSettings Settings { get; }
        IODataAdapter Adapter { get; }
        IMetadata Metadata { get; }
        IPluralizer Pluralizer { get; }
        void Trace(string message, params object[] messageParams);
    }
}