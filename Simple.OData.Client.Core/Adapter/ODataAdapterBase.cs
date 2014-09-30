using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public abstract class ODataAdapterBase : IODataAdapter
    {
        public abstract AdapterVersion AdapterVersion { get; }
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract string GetODataVersionString();
        public abstract string ConvertValueToUriLiteral(object value);
        public abstract IMetadata GetMetadata();
        public abstract IResponseReader GetResponseReader();
        public abstract IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        public abstract IBatchWriter GetBatchWriter();

        public string ConvertKeyToUriLiteral(IDictionary<string, object> key)
        {
            var formattedKeyValues = key.Count == 1 ?
                string.Join(",", key.Select(x => ConvertValueToUriLiteral(x.Value))) :
                string.Join(",", key.Select(x => string.Format("{0}={1}", x.Key, ConvertValueToUriLiteral(x.Value))));
            return "(" + formattedKeyValues + ")";
        }
    }
}