using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Annotations;
using Microsoft.OData.Edm.Library;
using Microsoft.Spatial;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.V4.Adapter
{
    public class RequestWriter : RequestWriterBase
    {
        private readonly IEdmModel _model;

        public RequestWriter(ISession session, IEdmModel model, Lazy<IBatchWriter> deferredBatchWriter)
            : base(session, deferredBatchWriter)
        {
            _model = model;
        }

        protected override async Task<Stream> WriteEntryContentAsync(string method, string collection, IDictionary<string, object> entryData, string commandText)
        {
            IODataRequestMessage message = IsBatch
                ? await CreateOperationRequestMessageAsync(method, collection, entryData, commandText)
                : new ODataRequestMessage();

            var entityType = FindEntityType(collection);

            using (var messageWriter = new ODataMessageWriter(message, GetWriterSettings(),
                method == RestVerbs.Patch ? new EdmDeltaModel(_model, entityType, entryData.Keys) : _model))
            {
                if (method == RestVerbs.Delete)
                    return null;

                var contentId = _deferredBatchWriter != null ? _deferredBatchWriter.Value.GetContentId(entryData) : null;
                var entityCollection = _session.Metadata.GetConcreteEntityCollection(collection);
                var entryDetails = _session.Metadata.ParseEntryDetails(entityCollection.ActualName, entryData, contentId);
                var entityTypeNamespace = _session.Metadata.GetEntityCollectionTypeNamespace(collection);
                var entityTypeName = _session.Metadata.GetEntityCollectionTypeName(collection);

                var entryWriter = messageWriter.CreateODataEntryWriter();
                var entry = new Microsoft.OData.Core.ODataEntry();
                entry.TypeName = string.Join(".", entityTypeNamespace, entityTypeName);

                var typeProperties = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).Properties();

                entry.Properties = entryDetails.Properties.Select(x => new ODataProperty()
                {
                    Name = typeProperties.Single(y => Utils.NamesMatch(y.Name, x.Key, _session.Pluralizer)).Name,
                    Value = GetPropertyValue(typeProperties, x.Key, x.Value)
                }).ToList();

                entryWriter.WriteStart(entry);

                if (entryDetails.Links != null)
                {
                    foreach (var link in entryDetails.Links)
                    {
                        if (link.Value.Any(x => x.LinkData != null))
                        {
                            WriteLink(entryWriter, entry, link.Key, link.Value);
                        }
                    }
                }

                entryWriter.WriteEnd();

                return IsBatch ? null : message.GetStream();
            }
        }

        protected override async Task<Stream> WriteLinkContentAsync(string linkIdent)
        {
            var message = new ODataRequestMessage();
            using (var messageWriter = new ODataMessageWriter(message, GetWriterSettings(), _model))
            {
                var link = new ODataEntityReferenceLink { Url = Utils.CreateAbsoluteUri(_session.UrlBase, linkIdent) };
                messageWriter.WriteEntityReferenceLink(link);

                return message.GetStream();
            }
        }

        protected override string FormatLinkPath(string entryIdent, string navigationPropertyName, string linkIdent = null)
        {
            return linkIdent == null
                ? string.Format("{0}/{1}/$ref", entryIdent, navigationPropertyName)
                : string.Format("{0}/{1}/$ref?$id={2}", entryIdent, navigationPropertyName, linkIdent);
        }

        protected override void AssignHeaders(ODataRequest request)
        {
            if (request.ResultRequired)
            {
                request.Headers.Add(HttpLiteral.Prefer, HttpLiteral.ReturnRepresentation);
            }
            else
            {
                request.Headers.Add(HttpLiteral.Prefer, HttpLiteral.ReturnMinimal);
            }
        }

        private async Task<IODataRequestMessage> CreateOperationRequestMessageAsync(string method, string collection, IDictionary<string, object> entryData, string commandText)
        {
            if (!_deferredBatchWriter.IsValueCreated)
                await _deferredBatchWriter.Value.StartBatchAsync();

            var message = (await _deferredBatchWriter.Value.CreateOperationRequestMessageAsync(
                method, entryData, new Uri(_session.UrlBase + commandText))) as IODataRequestMessage;

            if (_session.Metadata.EntityCollectionTypeRequiresOptimisticConcurrencyCheck(collection) &&
                (method == RestVerbs.Put || method == RestVerbs.Patch || method == RestVerbs.Delete))
            {
                message.SetHeader(HttpLiteral.IfMatch, EntityTagHeaderValue.Any.Tag);
            }

            return message;
        }

        private IEdmEntityType FindEntityType(string collection)
        {
            var entityTypeNamespace = _session.Metadata.GetEntityCollectionTypeNamespace(collection);
            var entityTypeName = _session.Metadata.GetEntityCollectionTypeName(collection);
            return _model.FindDeclaredType(string.Join(".", entityTypeNamespace, entityTypeName)) as IEdmEntityType;
        }

        private void WriteLink(ODataWriter entryWriter, Microsoft.OData.Core.ODataEntry entry, string linkName, IEnumerable<ReferenceLink> links)
        {
            var navigationProperty = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).NavigationProperties()
                .Single(x => Utils.NamesMatch(x.Name, linkName, _session.Pluralizer));
            bool isCollection = navigationProperty.Partner.TargetMultiplicity() == EdmMultiplicity.Many;

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

            foreach (var referenceLink in links)
            {
                var linkKey = linkType.DeclaredKey;
                var linkEntry = referenceLink.LinkData.ToDictionary();
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
                    var linkSet = _model.SchemaElements
                        .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                        .SelectMany(x => (x as IEdmEntityContainer).EntitySets())
                        .Single(x => Utils.NamesMatch(x.EntityType().Name, linkType.Name, _session.Pluralizer));
                    var formattedKey = _session.Adapter.ConvertKeyToUriLiteral(
                        linkKey.ToDictionary(x => x.Name, x => linkEntry[x.Name]));
                    linkUri = linkSet.Name + formattedKey;
                }
                var link = new ODataEntityReferenceLink
                {
                    Url = Utils.CreateAbsoluteUri(_session.UrlBase, linkUri)
                };

                entryWriter.WriteEntityReferenceLink(link);
            }

            entryWriter.WriteEnd();
        }

        private ODataMessageWriterSettings GetWriterSettings()
        {
            var settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri()
                {
                    RequestUri = new Uri(_session.UrlBase),
                }, 
                Indent = true,
                DisableMessageStreamDisposal = !IsBatch,
            };
            switch (_session.PayloadFormat)
            {
                case ODataPayloadFormat.Atom:
                    settings.SetContentType(ODataFormat.Atom);
                    break;
                case ODataPayloadFormat.Json:
                default:
                    settings.SetContentType(ODataFormat.Json);
                    break;
            }
            return settings;
        }

        private object GetPropertyValue(IEnumerable<IEdmProperty> properties, string key, object value)
        {
            if (value == null)
                return value;

            var property = properties.Single(x => Utils.NamesMatch(x.Name, key, _session.Pluralizer));
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
                    var mappedTypes = _typeMap.Where(x => x.Value == (property.Type.Definition as IEdmPrimitiveType).PrimitiveKind);
                    if (mappedTypes.Any())
                    {
                        foreach (var mappedType in mappedTypes)
                        {
                            object result;
                            if (Utils.TryConvert(value, mappedType.Key, out result))
                                return result;
                        }
                        throw new FormatException(string.Format("Unable to convert value of type {0} to OData type {1}", value.GetType(), property.Type));
                    }
                    break;

                default:
                    return value;
            }
            return value;
        }

        private static readonly Dictionary<Type, EdmPrimitiveTypeKind> _typeMap = new[]
            {
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(string), EdmPrimitiveTypeKind.String),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(bool), EdmPrimitiveTypeKind.Boolean),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(bool?), EdmPrimitiveTypeKind.Boolean),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(byte), EdmPrimitiveTypeKind.Byte),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(byte?), EdmPrimitiveTypeKind.Byte),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(decimal), EdmPrimitiveTypeKind.Decimal),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(decimal?), EdmPrimitiveTypeKind.Decimal),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(double), EdmPrimitiveTypeKind.Double),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(double?), EdmPrimitiveTypeKind.Double),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(Guid), EdmPrimitiveTypeKind.Guid),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(Guid?), EdmPrimitiveTypeKind.Guid),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(short), EdmPrimitiveTypeKind.Int16),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(short?), EdmPrimitiveTypeKind.Int16),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(int), EdmPrimitiveTypeKind.Int32),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(int?), EdmPrimitiveTypeKind.Int32),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(long), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(long?), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(sbyte), EdmPrimitiveTypeKind.SByte),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(sbyte?), EdmPrimitiveTypeKind.SByte),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(float), EdmPrimitiveTypeKind.Single),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(float?), EdmPrimitiveTypeKind.Single),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(byte[]), EdmPrimitiveTypeKind.Binary),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(Stream), EdmPrimitiveTypeKind.Stream),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(Geography), EdmPrimitiveTypeKind.Geography),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyPoint), EdmPrimitiveTypeKind.GeographyPoint),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyLineString), EdmPrimitiveTypeKind.GeographyLineString),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyPolygon), EdmPrimitiveTypeKind.GeographyPolygon),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyCollection), EdmPrimitiveTypeKind.GeographyCollection),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyMultiLineString), EdmPrimitiveTypeKind.GeographyMultiLineString),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyMultiPoint), EdmPrimitiveTypeKind.GeographyMultiPoint),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeographyMultiPolygon), EdmPrimitiveTypeKind.GeographyMultiPolygon),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(Geometry), EdmPrimitiveTypeKind.Geometry),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryPoint), EdmPrimitiveTypeKind.GeometryPoint),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryLineString), EdmPrimitiveTypeKind.GeometryLineString),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryPolygon), EdmPrimitiveTypeKind.GeometryPolygon),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryCollection), EdmPrimitiveTypeKind.GeometryCollection),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryMultiLineString), EdmPrimitiveTypeKind.GeometryMultiLineString),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryMultiPoint), EdmPrimitiveTypeKind.GeometryMultiPoint),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(GeometryMultiPolygon), EdmPrimitiveTypeKind.GeometryMultiPolygon),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(DateTimeOffset?), EdmPrimitiveTypeKind.DateTimeOffset),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(TimeSpan), EdmPrimitiveTypeKind.Duration),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(TimeSpan?), EdmPrimitiveTypeKind.Duration),

                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(XElement), EdmPrimitiveTypeKind.String),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(ushort), EdmPrimitiveTypeKind.Int32),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(ushort?), EdmPrimitiveTypeKind.Int32),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(uint), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(uint?), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(ulong), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(ulong?), EdmPrimitiveTypeKind.Int64),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(char[]), EdmPrimitiveTypeKind.String),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(char), EdmPrimitiveTypeKind.String),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(char?), EdmPrimitiveTypeKind.String),
            }
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}