using System;
using System.Collections.Generic;
using System.Spatial;

using Microsoft.Data.Edm;

using Simple.OData.Client.Adapter;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class V3Adapter
    {
        public static void Reference() { }
    }
}

#pragma warning disable 1591

namespace Simple.OData.Client.V3.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;
        private IMetadata _metadata;

        public override AdapterVersion AdapterVersion => AdapterVersion.V3;

        public override ODataPayloadFormat DefaultPayloadFormat => ODataPayloadFormat.Atom;

        public ODataAdapter(ISession session, IODataModelAdapter modelAdapter)
        {
            _session = session;
            ProtocolVersion = modelAdapter.ProtocolVersion;
            Model = modelAdapter.Model as IEdmModel;

            session.TypeCache.Converter.RegisterTypeConverter(typeof(GeographyPoint), TypeConverters.CreateGeographyPoint);
            session.TypeCache.Converter.RegisterTypeConverter(typeof(GeometryPoint), TypeConverters.CreateGeometryPoint);
            session.TypeCache.Converter.RegisterTypeConverter(typeof(DateTime), TypeConverters.ConvertToEdmDate);
            session.TypeCache.Converter.RegisterTypeConverter(typeof(DateTimeOffset), TypeConverters.ConvertToEdmDate);
        }

        public new IEdmModel Model
        {
            get => base.Model as IEdmModel;
            set
            {
                base.Model = value;
                // Ensure we replace the cache on change of model
                _metadata = null;
            }
        }

        public override string GetODataVersionString()
        {
            switch (ProtocolVersion)
            {
                case ODataProtocolVersion.V1:
                    return "V1";
                case ODataProtocolVersion.V2:
                    return "V2";
                case ODataProtocolVersion.V3:
                    return "V3";
            }
            throw new InvalidOperationException($"Unsupported OData protocol version: \"{this.ProtocolVersion}\"");
        }

        public override IMetadata GetMetadata()
        {
            // TODO: Should use a MetadataFactory here 
            return _metadata ?? (_metadata = new MetadataCache(new Metadata(Model, _session.Settings.NameMatchResolver, _session.Settings.IgnoreUnmappedProperties, _session.Settings.UnqualifiedNameCall)));
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