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
            var entry = new Microsoft.Data.OData.ODataEntry();
            entry.TypeName = string.Join(".", entityTypeNamespace, entityTypeName);
            var typeProperties = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).Properties();
            entry.Properties = properties.Select(x => new ODataProperty()
            {
                Name = typeProperties.Single(y => Utils.NamesAreEqual(y.Name, x.Key)).Name,
                Value = GetPropertyValue(typeProperties, x.Key, x.Value)
            });
            
            entryWriter.WriteStart(entry);

            if (associationsByValue != null)
            {
                foreach (var association in associationsByValue)
                {
                    if (association.Value == null)
                        continue;

                    var property = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).NavigationProperties()
                        .Single(x => Utils.NamesAreEqual(x.Name, association.Key));
                    var link = new ODataNavigationLink()
                    {
                        Name = association.Key,
                        IsCollection = property.Partner.Multiplicity() == EdmMultiplicity.Many,
                        Url = new Uri("", UriKind.Relative),
                    };

                    entryWriter.WriteStart(link);
                    entryWriter.WriteEnd();
                }
            }

            entryWriter.WriteEnd();

            return Utils.StreamToString(message.GetStream());
        }

        private object GetPropertyValue(IEnumerable<IEdmProperty> properties, string key, object value)
        {
            if (value == null)
                return value;

            var property = properties.Single(x => Utils.NamesAreEqual(x.Name, key));
            switch (property.Type.TypeKind())
            {
                case EdmTypeKind.Complex:
                    value = new ODataComplexValue()
                    {
                        TypeName = property.Type.FullName(),
                        Properties = (value as IDictionary<string, object>).Select(x => new ODataProperty()
                        {
                            Name = x.Key,
                            Value = GetPropertyValue(property.Type.AsComplex().StructuralProperties(), x.Key, x.Value),
                        }),
                    };
                    break;

                case EdmTypeKind.Collection:
                    value = new ODataCollectionValue()
                    {
                        TypeName = property.Type.FullName(),
                        Items = (value as IEnumerable<object>).Select(x => GetPropertyValue(
                            property.Type.AsCollection().AsStructured().StructuralProperties(), property.Name, x)),
                    };
                    break;

                case EdmTypeKind.Primitive:
                default:
                    return value;
            }
            return value;
        }
    }
}