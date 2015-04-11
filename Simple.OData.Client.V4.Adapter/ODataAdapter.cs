using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Spatial;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class V4Adapter
    {
        public static void Reference() { }
    }
}

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;

        public override AdapterVersion AdapterVersion { get { return AdapterVersion.V4; } }

        public override ODataPayloadFormat DefaultPayloadFormat
        {
            get { return ODataPayloadFormat.Json; }
        }

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        private ODataAdapter(ISession session, string protocolVersion)
        {
            _session = session;
            ProtocolVersion = protocolVersion;

            CustomConverters.RegisterTypeConverter(typeof(GeographyPoint), TypeConverters.CreateGeographyPoint);
            CustomConverters.RegisterTypeConverter(typeof(GeometryPoint), TypeConverters.CreateGeometryPoint);
        }

        public ODataAdapter(ISession session, string protocolVersion, HttpResponseMessage response)
            : this(session, protocolVersion)
        {
            var readerSettings = new ODataMessageReaderSettings
            {
                MessageQuotas = { MaxReceivedMessageSize = Int32.MaxValue }
            };
            using (var messageReader = new ODataMessageReader(new ODataResponseMessage(response), readerSettings))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataAdapter(ISession session, string protocolVersion, string metadataString)
            : this(session, protocolVersion)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override string GetODataVersionString()
        {
            switch (this.ProtocolVersion)
            {
                case ODataProtocolVersion.V4:
                    return "V4";
            }
            throw new InvalidOperationException(string.Format("Unsupported OData protocol version: \"{0}\"", this.ProtocolVersion));
        }

        public override string ConvertValueToUriLiteral(object value)
        {
            return value is ODataExpression
                ? (value as ODataExpression).AsString(_session)
                : ODataUriUtils.ConvertToUriLiteral(value,
                    (ODataVersion)Enum.Parse(typeof(ODataVersion), this.GetODataVersionString(), false), this.Model);
        }

        public override FunctionFormat FunctionFormat
        {
            get { return FunctionFormat.Key; }
        }

        public override IMetadata GetMetadata()
        {
            return new Metadata(_session, Model);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReader(_session, Model);
        }

        public override IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter)
        {
            return new RequestWriter(_session, Model, deferredBatchWriter);
        }

        public override IBatchWriter GetBatchWriter()
        {
            return new BatchWriter(_session);
        }

        public override void FormatCommandClauses(
            IList<string> commandClauses,
            EntityCollection entityCollection,
            IList<string> expandAssociations,
            IList<string> selectColumns,
            IList<KeyValuePair<string, bool>> orderbyColumns,
            bool includeCount)
        {
            if (expandAssociations.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Expand,
                    string.Join(",", expandAssociations.Select(x =>
                        FormatExpandSelectOrderByItem(x, entityCollection, selectColumns, orderbyColumns)))));
            }

            selectColumns = selectColumns.Where(x => !x.Contains("/")).ToList();
            if (selectColumns.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Select,
                    string.Join(",", selectColumns.Select(x => FormatSelectItem(x, entityCollection)))));
            }

            orderbyColumns = orderbyColumns.Where(x => !x.Key.Contains("/")).ToList();
            if (orderbyColumns.Any())
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.OrderBy,
                    string.Join(",", orderbyColumns.Select(x => FormatOrderByItem(x, entityCollection)))));
            }

            if (includeCount)
            {
                commandClauses.Add(string.Format("{0}={1}", ODataLiteral.Count, ODataLiteral.True));
            }
        }

        private string FormatExpandSelectOrderByItem(string path, EntityCollection entityCollection,
            IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns)
        {
            var items = path.Split('/');
            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());

            var text = string.Empty;
            if (items.Count() == 1)
            {
                selectColumns = selectColumns
                        .Where(x => x.Contains("/") && x.Split('/').First() == associationName)
                        .Select(x => string.Join("/", x.Split('/').Skip(1))).ToList();
                orderbyColumns = orderbyColumns
                        .Where(x => x.Key.Contains("/") && x.Key.Split('/').First() == associationName)
                        .Select(x => new KeyValuePair<string, bool>(
                            string.Join("/", x.Key.Split('/').Skip(1)), x.Value)).ToList();

                if (selectColumns.Any())
                {
                    var columns = string.Join(",", selectColumns.Where(x => !x.Contains("/")).ToList());
                    text += string.Format("{0}({1}={2})", associationName, ODataLiteral.Select, columns);
                }
                if (orderbyColumns.Any())
                {
                    var columns = string.Join(",", orderbyColumns.Where(x => !x.Key.Contains("/"))
                        .Select(x => x.Key + (x.Value ? " desc" : string.Empty)).ToList());
                    if (!string.IsNullOrEmpty(text)) text += ",";
                    text += string.Format("{0}({1}={2})", associationName, ODataLiteral.OrderBy, columns);
                }
                return string.IsNullOrEmpty(text) ? associationName : text;
            }
            else
            {
                path = path.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                    _session.Metadata.GetNavigationPropertyPartnerName(entityCollection.Name, associationName));

                selectColumns = selectColumns
                        .Where(x => x.Contains("/") && x.Split('/').First() == items.First())
                        .Select(x => string.Join("/", x.Split('/').Skip(1))).ToList();
                orderbyColumns = orderbyColumns
                        .Where(x => x.Key.Contains("/") && x.Key.Split('/').First() == items.First())
                        .Select(x => new KeyValuePair<string, bool>(
                            string.Join("/", x.Key.Split('/').Skip(1)), x.Value)).ToList();

                if (selectColumns.Any())
                {
                    selectColumns = selectColumns.Where(x => !x.Contains("/")).ToList();
                    text += string.Format("{0}({1}={2})",
                        associationName, ODataLiteral.Expand,
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns));
                }
                if (orderbyColumns.Any())
                {
                    orderbyColumns = orderbyColumns.Where(x => !x.Key.Contains("/")).ToList();
                    if (!string.IsNullOrEmpty(text)) text += ",";
                    text += string.Format("{0}({1}={2})",
                        associationName, ODataLiteral.Expand,
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns));
                }

                return string.IsNullOrEmpty(text)
                    ? string.Format("{0}({1}={2})", associationName, ODataLiteral.Expand,
                        FormatExpandSelectOrderByItem(path, entityCollection, selectColumns, orderbyColumns))
                    : text;
            }
        }
    }
}