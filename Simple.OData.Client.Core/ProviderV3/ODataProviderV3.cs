using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
    class ODataProviderV3
    {
        public EdmSchema CreateEdmSchema(HttpResponseMessage response)
        {
            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response)))
            {
                var model = messageReader.ReadMetadataDocument();
                return new EdmSchema(new EdmModelParserV3(model));
            }
        }
    }
}