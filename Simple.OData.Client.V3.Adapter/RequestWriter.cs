using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Spatial;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client.V3.Adapter
{
    public class RequestWriter : RequestWriterBase
    {
        private readonly IEdmModel _model;

        public RequestWriter(ISession session, IEdmModel model, Lazy<IBatchWriter> deferredBatchWriter)
            : base(session, deferredBatchWriter)
        {
            _model = model;
        }

        protected override async Task<Stream> WriteEntryContentAsync(string method, string collection, string commandText, IDictionary<string, object> entryData)
        {
#if SILVERLIGHT
            IODataRequestMessage
#else
            IODataRequestMessageAsync
#endif
                message = IsBatch
                ? await CreateOperationRequestMessageAsync(method, collection, entryData, commandText)
                : new ODataRequestMessage();

            var entityType = _model.FindDeclaredType(
                _session.Metadata.GetEntityCollectionQualifiedTypeName(collection)) as IEdmEntityType;
            var model = method == RestVerbs.Patch ? new EdmDeltaModel(_model, entityType, entryData.Keys) : _model;

            using (var messageWriter = new ODataMessageWriter(message, GetWriterSettings(), model))
            {
                if (method == RestVerbs.Get || method == RestVerbs.Delete)
                    return null;

                var contentId = _deferredBatchWriter != null ? _deferredBatchWriter.Value.GetContentId(entryData) : null;
                var entityCollection = _session.Metadata.GetEntityCollection(collection);
                var entryDetails = _session.Metadata.ParseEntryDetails(entityCollection.Name, entryData, contentId);

                var entryWriter = messageWriter.CreateODataEntryWriter();
                var entry = new Microsoft.Data.OData.ODataEntry();
                entry.TypeName = entityType.FullName();

                var typeProperties = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).Properties();
                entry.Properties = entryDetails.Properties.Select(x => new ODataProperty()
                {
                    Name = typeProperties.BestMatch(y => y.Name, x.Key, _session.Pluralizer).Name,
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

                if (IsBatch)
                    return null;

#if SILVERLIGHT
                return message.GetStream();
#else
                return await message.GetStreamAsync();
#endif
            }
        }

#pragma warning disable 1998
        protected override async Task<Stream> WriteLinkContentAsync(string linkIdent)
        {
            var message = new ODataRequestMessage();
            using (var messageWriter = new ODataMessageWriter(message, GetWriterSettings(), _model))
            {
                var link = new ODataEntityReferenceLink { Url = Utils.CreateAbsoluteUri(_session.Settings.BaseUri.AbsoluteUri, linkIdent) };
                messageWriter.WriteEntityReferenceLink(link);

#if SILVERLIGHT
                return message.GetStream();
#else
                return await message.GetStreamAsync();
#endif
            }
        }
#pragma warning restore 1998

        protected override async Task<Stream> WriteActionContentAsync(string actionName, IDictionary<string, object> parameters)
        {
            var message = new ODataRequestMessage();
            using (var messageWriter = new ODataMessageWriter(message, GetWriterSettings(), _model))
            {
                var action = _model.SchemaElements
                    .Where(x => x.SchemaElementKind == EdmSchemaElementKind.EntityContainer)
                    .SelectMany(x => (x as IEdmEntityContainer).FunctionImports())
                    .BestMatch(x => x.Name, actionName, _session.Pluralizer);
                var parameterWriter = messageWriter.CreateODataParameterWriter(action);

                parameterWriter.WriteStart();

                foreach (var parameter in parameters)
                {
                    if (!(parameter.Value is string) && parameter.Value is IEnumerable)
                    {
                        var collectionWriter = parameterWriter.CreateCollectionWriter(parameter.Key);
                        collectionWriter.WriteStart(new ODataCollectionStart());
                        foreach (var item in parameter.Value as IEnumerable)
                        {
                            collectionWriter.WriteItem(item);
                        }
                        collectionWriter.WriteEnd();
                    }
                    else
                    {
                        parameterWriter.WriteValue(parameter.Key, parameter.Value);
                    }
                }

                parameterWriter.WriteEnd();
                return await message.GetStreamAsync();
            }
        }

        protected override string FormatLinkPath(string entryIdent, string navigationPropertyName, string linkIdent = null)
        {
            return linkIdent == null
                ? string.Format("{0}/$links/{1}", entryIdent, navigationPropertyName)
                : string.Format("{0}/$links/{1}", entryIdent, linkIdent);
        }

        protected override void AssignHeaders(ODataRequest request)
        {
            if (request.ResultRequired)
            {
                request.Headers.Add(HttpLiteral.Prefer, HttpLiteral.ReturnContent);
            }
            else
            {
                request.Headers.Add(HttpLiteral.Prefer, HttpLiteral.ReturnNoContent);
            }
        }

        private ODataMessageWriterSettings GetWriterSettings()
        {
            var settings = new ODataMessageWriterSettings()
            {
                BaseUri = _session.Settings.BaseUri,
                Indent = true,
                DisableMessageStreamDisposal = !IsBatch,
            };
            ODataFormat contentType;
            switch (_session.Settings.PayloadFormat)
            {
                case ODataPayloadFormat.Atom:
                default:
                    contentType = ODataFormat.Atom;
                    break;
                case ODataPayloadFormat.Json:
                    switch (_session.Adapter.ProtocolVersion)
                    {
                        case ODataProtocolVersion.V1:
                        case ODataProtocolVersion.V2:
                            contentType = ODataFormat.VerboseJson;
                            break;
                        default:
                            contentType = ODataFormat.Json;
                            break;
                    }
                    break;
            }
            settings.SetContentType(contentType);
            return settings;
        }

#if SILVERLIGHT
        private async Task<IODataRequestMessage> CreateOperationRequestMessageAsync(string method, string collection, IDictionary<string, object> entryData, string commandText)
#else
        private async Task<IODataRequestMessageAsync> CreateOperationRequestMessageAsync(string method, string collection, IDictionary<string, object> entryData, string commandText)
#endif
        {
            if (!_deferredBatchWriter.IsValueCreated)
                await _deferredBatchWriter.Value.StartBatchAsync();

            var message = (await _deferredBatchWriter.Value.CreateOperationRequestMessageAsync(
                method, collection, entryData, Utils.CreateAbsoluteUri(_session.Settings.BaseUri.AbsoluteUri, commandText))) 
#if SILVERLIGHT
                as IODataRequestMessage;
#else
                as IODataRequestMessageAsync;
#endif

            return message;
        }

        private void WriteLink(ODataWriter entryWriter, Microsoft.Data.OData.ODataEntry entry, string linkName, IEnumerable<ReferenceLink> links)
        {
            var navigationProperty = (_model.FindDeclaredType(entry.TypeName) as IEdmEntityType).NavigationProperties()
                .BestMatch(x => x.Name, linkName, _session.Pluralizer);
            bool isCollection = navigationProperty.Type.Definition.TypeKind == EdmTypeKind.Collection;

            var linkType = GetNavigationPropertyEntityType(navigationProperty);
            var linkTypeWithKey = linkType;
            while (linkTypeWithKey.DeclaredKey == null && linkTypeWithKey.BaseEntityType() != null)
            {
                linkTypeWithKey = linkTypeWithKey.BaseEntityType();
            }

            entryWriter.WriteStart(new ODataNavigationLink()
            {
                Name = linkName,
                IsCollection = isCollection,
                Url = new Uri("http://schemas.microsoft.com/ado/2007/08/dataservices/related/" + linkType, UriKind.Absolute),
            });

            foreach (var referenceLink in links)
            {
                var linkKey = linkTypeWithKey.DeclaredKey;
                var linkEntry = referenceLink.LinkData.ToDictionary();
                var contentId = GetContentId(referenceLink);
                string linkUri;
                if (contentId != null)
                {
                    linkUri = "$" + contentId;
                }
                else
                {
                    var linkSet = _model.EntityContainers()
                        .SelectMany(x => x.EntitySets())
                        .BestMatch(x => x.ElementType.Name, linkTypeWithKey.Name, _session.Pluralizer);
                    var formattedKey = _session.Adapter.ConvertKeyValuesToUriLiteral(
                        linkKey.ToDictionary(x => x.Name, x => linkEntry[x.Name]), true);
                    linkUri = linkSet.Name + formattedKey;
                }
                var link = new ODataEntityReferenceLink
                {
                    Url = Utils.CreateAbsoluteUri(_session.Settings.BaseUri.AbsoluteUri, linkUri)
                };

                entryWriter.WriteEntityReferenceLink(link);
            }

            entryWriter.WriteEnd();
        }

        private static IEdmEntityType GetNavigationPropertyEntityType(IEdmNavigationProperty navigationProperty)
        {
            if (navigationProperty.Type.Definition.TypeKind == EdmTypeKind.Collection)
                return (navigationProperty.Type.Definition as IEdmCollectionType).ElementType.Definition as IEdmEntityType;
            else
                return navigationProperty.Type.Definition as IEdmEntityType;
        }

        private object GetPropertyValue(IEnumerable<IEdmProperty> properties, string key, object value)
        {
            var property = properties.BestMatch(x => x.Name, key, _session.Pluralizer);
            return GetPropertyValue(property.Type, value);
        }

        private object GetPropertyValue(IEdmTypeReference propertyType, object value)
        {
            if (value == null)
                return value;

            switch (propertyType.TypeKind())
            {
                case EdmTypeKind.Complex:
                    return new ODataComplexValue()
                    {
                        TypeName = propertyType.FullName(),
                        Properties = value.ToDictionary().Select(x => new ODataProperty()
                        {
                            Name = x.Key,
                            Value = GetPropertyValue(propertyType.AsComplex().StructuralProperties(), x.Key, x.Value),
                        }),
                    };

                case EdmTypeKind.Collection:
                    var collection = propertyType.AsCollection();
                    return new ODataCollectionValue()
                    {
                        TypeName = propertyType.FullName(),
                        Items = (value as IEnumerable<object>).Select(x => GetPropertyValue(collection.ElementType(), x)),
                    };

                case EdmTypeKind.Primitive:
                    var mappedTypes = _typeMap.Where(x => x.Value == (propertyType.Definition as IEdmPrimitiveType).PrimitiveKind);
                    if (mappedTypes.Any())
                    {
                        foreach (var mappedType in mappedTypes)
                        {
                            object result;
                            if (Client.Utils.TryConvert(value, mappedType.Key, out result))
                                return result;
                        }
                        throw new FormatException(string.Format("Unable to convert value of type {0} to OData type {1}", value.GetType(), propertyType));
                    }
                    return value;

                case EdmTypeKind.Enum:
                    return value.ToString();

                default:
                    return value;
            }
        }

        private static readonly Dictionary<Type, EdmPrimitiveTypeKind> _typeMap = new []
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
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(DateTime), EdmPrimitiveTypeKind.DateTime),
                new KeyValuePair<Type, EdmPrimitiveTypeKind>(typeof(DateTime?), EdmPrimitiveTypeKind.DateTime),

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