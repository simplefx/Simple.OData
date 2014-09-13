using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class RequestWriterV3 : IRequestWriter
    {
        private readonly ISession _session;
        private readonly IEdmModel _model;
        private readonly Lazy<IBatchWriter> _deferredBatchWriter;

        public RequestWriterV3(ISession session, IEdmModel model, Lazy<IBatchWriter> deferredBatchWriter)
        {
            _session = session;
            _model = model;
            _deferredBatchWriter = deferredBatchWriter;
        }

        public async Task<Stream> WriteEntryContentAsync(string method, string collection, IDictionary<string, object> entryData, string commandText)
        {
            var writerSettings = new ODataMessageWriterSettings() { BaseUri = new Uri(_session.UrlBase), Indent = true };
            IODataRequestMessage message;
            if (_deferredBatchWriter != null)
            {
                if (!_deferredBatchWriter.IsValueCreated)
                    await _deferredBatchWriter.Value.StartBatchAsync();
                message = (await _deferredBatchWriter.Value.CreateOperationRequestMessageAsync(
                    method, new Uri(_session.UrlBase + commandText))) as IODataRequestMessage;
                if (method != RestVerbs.Delete)
                {
                    var contentId = _deferredBatchWriter.Value.NextContentId();
                    _deferredBatchWriter.Value.MapContentId(entryData, contentId);
                    message.SetHeader(HttpLiteral.ContentId, contentId);
                }

                if (_session.Provider.GetMetadata().EntitySetTypeRequiresOptimisticConcurrencyCheck(collection) &&
                    (method == RestVerbs.Put || method == RestVerbs.Patch || method == RestVerbs.Delete))
                {
                    message.SetHeader(HttpLiteral.IfMatch, EntityTagHeaderValue.Any.Tag);
                }
            }
            else
            {
                message = new ODataV3RequestMessage();
            }

            Func<IDictionary<string, object>, string> resolveContentIdFunc = null;
            if (_deferredBatchWriter != null)
            {
                resolveContentIdFunc = _deferredBatchWriter.Value.GetContentId;
            }

            using (var messageWriter = new ODataMessageWriter(message, writerSettings, _model))
            {
                if (method == RestVerbs.Delete)
                    return null;

                var entitySet = (_session as Session).MetadataCache.FindConcreteEntitySet(collection);
                var entryDetails = Utils.ParseEntryDetails(entitySet, entryData, resolveContentIdFunc);
                var entityTypeNamespace = _session.Provider.GetMetadata().GetEntitySetTypeNamespace(collection);
                var entityTypeName = _session.Provider.GetMetadata().GetEntitySetTypeName(collection);

                var entryWriter = messageWriter.CreateODataEntryWriter();
                var entry = new Microsoft.Data.OData.ODataEntry();
                entry.TypeName = string.Join(".", entityTypeNamespace, entityTypeName);

                var typeProperties = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).Properties();
                entry.Properties = entryDetails.Properties.Select(x => new ODataProperty()
                {
                    Name = typeProperties.Single(y => Utils.NamesAreEqual(y.Name, x.Key, _session.Pluralizer)).Name,
                    Value = GetPropertyValue(typeProperties, x.Key, x.Value)
                }).ToList();

                entryWriter.WriteStart(entry);

                if (entryDetails.Links != null)
                {
                    foreach (var link in entryDetails.Links)
                    {
                        if (link.LinkData != null)
                            WriteLink(entryWriter, entry, link.LinkName, link.LinkData);
                    }
                }

                entryWriter.WriteEnd();
            }

            if (_deferredBatchWriter != null)
            {
                return null;
            }
            else
            {
                return Utils.CloneStream(message.GetStream());
            }
        }

        public async Task<Stream> WriteLinkContentAsync(string linkPath)
        {
            var writerSettings = new ODataMessageWriterSettings() { BaseUri = new Uri(_session.UrlBase), Indent = true };
            var message = new ODataV3RequestMessage();
            using (var messageWriter = new ODataMessageWriter(message, writerSettings, _model))
            {
                var link = new ODataEntityReferenceLink { Url = new Uri(linkPath, UriKind.Relative) };
                messageWriter.WriteEntityReferenceLink(link);

                return Utils.CloneStream(message.GetStream());
            }
        }

        private void WriteLink(ODataWriter entryWriter, Microsoft.Data.OData.ODataEntry entry, string linkName, object linkData)
        {
            var navigationProperty = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).NavigationProperties()
                .Single(x => Utils.NamesAreEqual(x.Name, linkName, _session.Pluralizer));
            bool isCollection = navigationProperty.Partner.Multiplicity() == EdmMultiplicity.Many;

            IEdmEntityType linkType;
            if (navigationProperty.Type.Definition.TypeKind == EdmTypeKind.Collection)
                linkType = (navigationProperty.Type.Definition as IEdmCollectionType).ElementType.Definition as IEdmEntityType;
            else
                linkType = navigationProperty.Type.Definition as IEdmEntityType;

            entryWriter.WriteStart(new ODataNavigationLink()
            {
                Name = linkName,
                IsCollection = isCollection,
                Url = new Uri("http://schemas.microsoft.com/ado/2007/08/dataservices/related/" + linkType, UriKind.Absolute),
            });

            var linkKey = linkType.DeclaredKey;
            var linkEntry = linkData.ToDictionary();
            string contentId = null;
            if (_deferredBatchWriter != null)
            {
                contentId = _deferredBatchWriter.Value.GetContentId(linkEntry);
            }
            string linkUri;
            if (contentId != null)
            {
                linkUri = "$" + contentId;
            }
            else
            {
                var formattedKey = "(" + string.Join(".", linkKey.Select(x => new ValueFormatter().FormatContentValue(linkEntry[x.Name]))) + ")";
                var linkSet = _model.EntityContainers().SelectMany(x => x.EntitySets())
                    .Single(x => Utils.NamesAreEqual(x.ElementType.Name, linkType.Name, _session.Pluralizer));
                linkUri = linkSet.Name + formattedKey;
            }
            var link = new ODataEntityReferenceLink
            {
                Url = new Uri(linkUri, UriKind.Relative)
            };

            entryWriter.WriteEntityReferenceLink(link);

            entryWriter.WriteEnd();
        }

        private object GetPropertyValue(IEnumerable<IEdmProperty> properties, string key, object value)
        {
            if (value == null)
                return value;

            var property = properties.Single(x => Utils.NamesAreEqual(x.Name, key, _session.Pluralizer));
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