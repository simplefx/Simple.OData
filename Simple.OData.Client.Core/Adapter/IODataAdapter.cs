using System;

namespace Simple.OData.Client
{
    public interface IODataAdapter
    {
        AdapterVersion AdapterVersion { get; }
        string ProtocolVersion { get; set; }
        object Model { get; set; }

        string GetODataVersionString();
        IMetadata GetMetadata();
        IResponseReader GetResponseReader();
        IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        IBatchWriter GetBatchWriter();
    }
}