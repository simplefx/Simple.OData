using System;
using System.Net.Http;
using Microsoft.OData.Core;

namespace Simple.OData.Client
{
    class ODataProviderV4
    {
        public EdmSchema CreateEdmSchema(HttpResponseMessage response)
        {
            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                var model = messageReader.ReadMetadataDocument();
                return new EdmSchema(new EdmModelParserV4(model));
            }
        }
    }
}