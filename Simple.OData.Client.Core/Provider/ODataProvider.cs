using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class ODataProvider
    {
        public string ProtocolVersion { get; set; }
        public object Model { get; set; }

        public abstract IMetadata GetMetadata();
        public abstract IResponseReader GetResponseReader();
        public abstract IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        public abstract IBatchWriter GetBatchWriter();
    }
}