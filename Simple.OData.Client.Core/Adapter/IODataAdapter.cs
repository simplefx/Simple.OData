using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public interface IODataAdapter
    {
        AdapterVersion AdapterVersion { get; }
        ODataPayloadFormat DefaultPayloadFormat { get; }
        string ProtocolVersion { get; set; }
        object Model { get; set; }

        string GetODataVersionString();
        string ConvertValueToUriLiteral(object value);
        string ConvertKeyValuesToUriLiteral(IDictionary<string, object> key, bool skipKeyNameForSingleValue);
        FunctionFormat FunctionFormat { get; }
        void FormatCommandClauses(IList<string> commandClauses, EntityCollection entityCollection,
            IList<string> expandAssociations, IList<string> selectColumns, IList<KeyValuePair<string, bool>> orderbyColumns, bool includeCount);

        IMetadata GetMetadata();
        IResponseReader GetResponseReader();
        IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter);
        IBatchWriter GetBatchWriter();
    }
}