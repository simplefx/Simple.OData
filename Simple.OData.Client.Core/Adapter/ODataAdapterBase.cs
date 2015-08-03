using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public abstract class ODataAdapterBase : IODataAdapter
    {
        public abstract AdapterVersion AdapterVersion { get; }
        public abstract ODataPayloadFormat DefaultPayloadFormat { get; }
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract string GetODataVersionString();

        public abstract IMetadata GetMetadata();
        public abstract ICommandFormatter GetCommandFormatter();
        public abstract IResponseReader GetResponseReader();
        public abstract IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        public abstract IBatchWriter GetBatchWriter(IDictionary<object, IDictionary<string, object>> batchEntries);
    }
}