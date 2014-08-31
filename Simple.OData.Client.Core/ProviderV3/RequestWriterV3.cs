using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
    public class RequestWriterV3 : IRequestWriter
    {
        private readonly string _urlBase;
        private readonly IEdmModel _model;

        public RequestWriterV3(string urlBase, IEdmModel model)
        {
            _urlBase = urlBase;
            _model = model;
        }

        public string CreateEntry(string entityTypeNamespace, string entityTypeName, 
            IDictionary<string, object> properties,
            IDictionary<string, object> associationsByValue,
            IDictionary<string, int> associationsByContentId)
        {
            // TODO: check dispose
            var writerSettings = new ODataMessageWriterSettings();
            var message = new ODataV3RequestMessage(null, null);
            var messageWriter = new ODataMessageWriter(message, writerSettings, _model);
            var entryWriter = messageWriter.CreateODataEntryWriter();
            var odataEntry = new Microsoft.Data.OData.ODataEntry();
            odataEntry.TypeName = string.Join(".", entityTypeNamespace, entityTypeName);
            odataEntry.Properties = properties.Select(x => new ODataProperty() { Name = x.Key, Value = x.Value });
            
            entryWriter.WriteStart(odataEntry);

            foreach (var association in associationsByValue)
            {
                //var link = new ODataNavigationLink()
                //{
                //    Name = association.Key,
                //    Url = new Uri(new Uri(_urlBase, UriKind.Absolute), association.Value.ToString())
                //};

                //entryWriter.WriteStart(link);
                //entryWriter.WriteEnd();
            }

            entryWriter.WriteEnd();

            return Utils.StreamToString(message.GetStream());
        }
    }
}