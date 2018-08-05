using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.Spatial;

using Simple.OData.Client.Adapter;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class V4Adapter
    {
        public static void Reference() { }
    }
}

#pragma warning disable 1591

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;
        private IMetadata _metadata;

        public override AdapterVersion AdapterVersion => AdapterVersion.V4;

        public override ODataPayloadFormat DefaultPayloadFormat => ODataPayloadFormat.Json;

        public new IEdmModel Model
        {
            get => base.Model as IEdmModel;
            set
            {
                base.Model = value;
                _metadata = null;
            }
        }

        public ODataAdapter(ISession session, IODataModelAdapter modelAdapter)
        {
            _session = session;
            ProtocolVersion = modelAdapter.ProtocolVersion;
            Model = modelAdapter.Model as IEdmModel;

            CustomConverters.RegisterTypeConverter(typeof(GeographyPoint), TypeConverters.CreateGeographyPoint);
            CustomConverters.RegisterTypeConverter(typeof(GeometryPoint), TypeConverters.CreateGeometryPoint);
        }

        public override string GetODataVersionString()
        {
            switch (this.ProtocolVersion)
            {
                case ODataProtocolVersion.V4:
                    return "V4";
            }
            throw new InvalidOperationException($"Unsupported OData protocol version: \"{this.ProtocolVersion}\"");
        }

        public override IMetadata GetMetadata()
        {
            return _metadata ?? (_metadata = new CachedMetadata(new Metadata(_session, Model)));
        }

        public override ICommandFormatter GetCommandFormatter()
        {
            return new CommandFormatter(_session);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReader(_session, Model);
        }

        public override IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter)
        {
            return new RequestWriter(_session, Model, deferredBatchWriter);
        }

        public override IBatchWriter GetBatchWriter(IDictionary<object, IDictionary<string, object>> batchEntries)
        {
            return new BatchWriter(_session, batchEntries);
        }
    }
}