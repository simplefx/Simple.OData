using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client.V3.Adapter
{
    public class ResponseReader : ResponseReaderBase
    {
        private readonly IEdmModel _model;
        private bool _hasResponse = false;

        public ResponseReader(ISession session, IEdmModel model)
            : base(session)
        {
            _model = model;
        }

        public override Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage)
        {
            return GetResponseAsync(new ODataResponseMessage(responseMessage));
        }

        protected override void ConvertEntry(ResponseNode entryNode, object entry)
        {
            if (entry != null)
            {
                var odataEntry = entry as Microsoft.Data.OData.ODataEntry;
                foreach (var property in odataEntry.Properties)
                {
                    entryNode.Entry.Add(property.Name, GetPropertyValue(property.Value));
                    entryNode.EntryAnnotations = CreateAnnotations(odataEntry);
                }
            }
        }

        private ODataEntryAnnotations CreateAnnotations(Microsoft.Data.OData.ODataEntry odataEntry)
        {
            return new ODataEntryAnnotations
            {
                Id = odataEntry.Id,
                TypeName = odataEntry.TypeName,
                ReadLink = odataEntry.ReadLink,
                EditLink = odataEntry.EditLink,
                ETag = odataEntry.ETag,
                AssociationLinks = odataEntry.AssociationLinks == null
                    ? null
                    : new List<ODataEntryAnnotations.AssociationLink>(
                    odataEntry.AssociationLinks.Select(x => new ODataEntryAnnotations.AssociationLink
                    {
                        Name = x.Name,
                        Uri = x.Url,
                    })),
                MediaResource = CreateAnnotations(odataEntry.MediaResource),
                InstanceAnnotations = odataEntry.InstanceAnnotations,
            };
        }

        private ODataMediaAnnotations CreateAnnotations(ODataStreamReferenceValue value)
        {
            return value == null ? null : new ODataMediaAnnotations
            {
                ContentType = value.ContentType,
                ReadLink = value.ReadLink,
                EditLink = value.EditLink,
                ETag = value.ETag,
            };
        }

#if SILVERLIGHT
        public async Task<ODataResponse> GetResponseAsync(IODataResponseMessage responseMessage)
#else
        public async Task<ODataResponse> GetResponseAsync(IODataResponseMessageAsync responseMessage)
#endif
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(responseMessage, readerSettings, _model))
            {
                var payloadKind = messageReader.DetectPayloadKind();
                if (payloadKind.Any(x => x.PayloadKind != ODataPayloadKind.Property))
                    _hasResponse = true;

                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Error))
                {
                    return ODataResponse.FromStatusCode(responseMessage.StatusCode);
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Value))
                {
                    if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Collection))
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
#if SILVERLIGHT
                        var text = Utils.StreamToString(responseMessage.GetStream());
#else
                        var text = Utils.StreamToString(await responseMessage.GetStreamAsync());
#endif
                        return ODataResponse.FromFeed(new[] { new Dictionary<string, object>()
                        {
                            { FluentCommand.ResultLiteral, text }
                        } });
                    }
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Batch))
                {
                    return await ReadResponse(messageReader.CreateODataBatchReader());
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Feed))
                {
                    return ReadResponse(messageReader.CreateODataFeedReader());
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Collection))
                {
                    return ReadResponse(messageReader.CreateODataCollectionReader());
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Property))
                {
                    var property = messageReader.ReadProperty();
                    if (property.Value != null && (property.Value.GetType() != typeof (string) || !string.IsNullOrEmpty(property.Value.ToString())))
                        _hasResponse = true;

                    if (_hasResponse)
                    {
                        return ODataResponse.FromFeed(new[] { new Dictionary<string, object>()
                        {
                            { property.Name ?? FluentCommand.ResultLiteral, GetPropertyValue(property.Value) }
                        } });
                    }
                    else
                    {
                        return ODataResponse.FromFeed(new Dictionary<string, object>[] {});
                    }
                }
                else
                {
                    return ReadResponse(messageReader.CreateODataEntryReader());
                }
            }
        }

        private async Task<ODataResponse> ReadResponse(ODataBatchReader odataReader)
        {
            var batch = new List<ODataResponse>();

            while (odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataBatchReaderState.ChangesetStart:
                        break;
                    case ODataBatchReaderState.Operation:
                        var operationMessage = odataReader.CreateOperationResponseMessage();
                        if (operationMessage.StatusCode == (int)HttpStatusCode.NoContent)
                            batch.Add(ODataResponse.FromStatusCode(operationMessage.StatusCode));
                        else
                            batch.Add(await GetResponseAsync(operationMessage));
                        break;
                    case ODataBatchReaderState.ChangesetEnd:
                        break;
                }
            }

            return ODataResponse.FromBatch(batch);
        }

        private ODataResponse ReadResponse(ODataCollectionReader odataReader)
        {
            var collection = new List<object>();

            while (odataReader.Read())
            {
                if (odataReader.State == ODataCollectionReaderState.Completed)
                    break;

                switch (odataReader.State)
                {
                    case ODataCollectionReaderState.CollectionStart:
                        break;

                    case ODataCollectionReaderState.Value:
                        collection.Add(GetPropertyValue(odataReader.Item));
                        break;

                    case ODataCollectionReaderState.CollectionEnd:
                        break;
                }
            }

            return ODataResponse.FromCollection(collection);
        }

        private ODataResponse ReadResponse(ODataReader odataReader)
        {
            ResponseNode rootNode = null;
            var nodeStack = new Stack<ResponseNode>();

            while (odataReader.Read())
            {
                if (odataReader.State == ODataReaderState.Completed)
                    break;

                switch (odataReader.State)
                {
                    case ODataReaderState.FeedStart:
                        StartFeed(nodeStack, CreateFeedDetails(odataReader.Item as ODataFeed));
                        break;

                    case ODataReaderState.FeedEnd:
                        EndFeed(nodeStack, CreateFeedDetails(odataReader.Item as ODataFeed), ref rootNode);
                        break;

                    case ODataReaderState.EntryStart:
                        StartEntry(nodeStack);
                        break;

                    case ODataReaderState.EntryEnd:
                        EndEntry(nodeStack, ref rootNode, odataReader.Item);
                        break;

                    case ODataReaderState.NavigationLinkStart:
                        StartNavigationLink(nodeStack, (odataReader.Item as ODataNavigationLink).Name);
                        break;

                    case ODataReaderState.NavigationLinkEnd:
                        EndNavigationLink(nodeStack);
                        break;
                }
            }

            return rootNode.Feed != null
                ? ODataResponse.FromFeed(rootNode.Feed, rootNode.FeedAnnotations, rootNode.FeedEntryAnnotations)
                : ODataResponse.FromEntry(rootNode.Entry, rootNode.EntryAnnotations);
        }

        private ODataFeedAnnotations CreateFeedDetails(ODataFeed feed)
        {
            return new ODataFeedAnnotations()
            {
                Id = feed.Id,
                Count = feed.Count,
                DeltaLink = feed.DeltaLink,
                NextPageLink = feed.NextPageLink,
                InstanceAnnotations = feed.InstanceAnnotations,
            };
        }

        private object GetPropertyValue(object value)
        {
            if (value is ODataComplexValue)
            {
                return (value as ODataComplexValue).Properties.ToDictionary(
                    x => x.Name, x => GetPropertyValue(x.Value));
            }
            else if (value is ODataCollectionValue)
            {
                return (value as ODataCollectionValue).Items.Cast<object>()
                    .Select(GetPropertyValue).ToList();
            }
            else if (value is ODataStreamReferenceValue)
            {
                return CreateAnnotations(value as ODataStreamReferenceValue);
            }
            else
            {
                return value;
            }
        }
    }
}