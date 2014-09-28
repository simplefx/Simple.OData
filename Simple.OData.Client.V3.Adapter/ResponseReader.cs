using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace Simple.OData.Client.V3.Adapter
{
    public class ResponseReader : IResponseReader
    {
        private readonly ISession _session;
        private readonly IEdmModel _model;

        public ResponseReader(ISession session, IEdmModel model)
        {
            _session = session;
            _model = model;
        }

        public Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage, bool includeResourceTypeInEntryProperties = false)
        {
            return GetResponseAsync(new ODataResponseMessage(responseMessage), includeResourceTypeInEntryProperties);
        }

#if SILVERLIGHT
        public async Task<ODataResponse> GetResponseAsync(IODataResponseMessage responseMessage, bool includeResourceTypeInEntryProperties = false)
#else
        public async Task<ODataResponse> GetResponseAsync(IODataResponseMessageAsync responseMessage, bool includeResourceTypeInEntryProperties = false)
#endif
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(responseMessage, readerSettings, _model))
            {
                var payloadKind = messageReader.DetectPayloadKind();
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Value))
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
                        var text = Client.Utils.StreamToString(await responseMessage.GetStreamAsync());
#endif
                        return ODataResponse.FromFeed(new[] { new Dictionary<string, object>() { { FluentCommand.ResultLiteral, text } } });
                    }
                }
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Feed))
                {
                    return ReadResponse(messageReader.CreateODataFeedReader(), includeResourceTypeInEntryProperties);
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Collection))
                {
                    return ReadResponse(messageReader.CreateODataCollectionReader(), includeResourceTypeInEntryProperties);
                }
                else if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Property))
                {
                    var property = messageReader.ReadProperty();
                    return ODataResponse.FromFeed(new[] { new Dictionary<string, object>() { { property.Name, GetPropertyValue(property.Value) } } });
                }
                else
                {
                    return ReadResponse(messageReader.CreateODataEntryReader(), includeResourceTypeInEntryProperties);
                }
            }
        }

        private ODataResponse ReadResponse(ODataCollectionReader odataReader, bool includeResourceTypeInEntryProperties)
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

        private ODataResponse ReadResponse(ODataReader odataReader, bool includeResourceTypeInEntryProperties)
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
                        nodeStack.Push(new ResponseNode
                        {
                            Feed = new List<IDictionary<string, object>>(),
                            TotalCount = (odataReader.Item as ODataFeed).Count
                        });
                        break;

                    case ODataReaderState.FeedEnd:
                        var feedNode = nodeStack.Pop();
                        var entries = feedNode.Feed;
                        if (nodeStack.Any())
                            nodeStack.Peek().Feed = entries;
                        else
                            rootNode = feedNode;
                        break;

                    case ODataReaderState.EntryStart:
                        nodeStack.Push(new ResponseNode
                        {
                            Entry = new Dictionary<string, object>()
                        });
                        break;

                    case ODataReaderState.EntryEnd:
                        var entry = (odataReader.Item as Microsoft.Data.OData.ODataEntry);
                        var entryNode = nodeStack.Pop();
                        if (entry != null)
                        {
                            foreach (var property in entry.Properties)
                            {
                                entryNode.Entry.Add(property.Name, GetPropertyValue(property.Value));
                            }
                            if (includeResourceTypeInEntryProperties)
                            {
                                var resourceType = entry.TypeName;
                                entryNode.Entry.Add(FluentCommand.ResourceTypeLiteral, resourceType.Split('.').Last());
                            }
                        }
                        if (nodeStack.Any())
                        {
                            if (nodeStack.Peek().Feed != null)
                                nodeStack.Peek().Feed.Add(entryNode.Entry);
                            else
                                nodeStack.Peek().Entry = entryNode.Entry;
                        }
                        else
                        {
                            rootNode = entryNode;
                        }
                        break;

                    case ODataReaderState.NavigationLinkStart:
                        var link = odataReader.Item as ODataNavigationLink;
                        nodeStack.Push(new ResponseNode
                        {
                            LinkName = link.Name,
                        });
                        break;

                    case ODataReaderState.NavigationLinkEnd:
                        var linkNode = nodeStack.Pop();
                        if (linkNode.Value != null)
                        {
                            var linkValue = linkNode.Value;
                            if (linkNode.Value is IDictionary<string, object> && !(linkNode.Value as IDictionary<string,object>).Any())
                                linkValue = null;
                            nodeStack.Peek().Entry.Add(linkNode.LinkName, linkValue);
                        }
                        break;
                }
            }

            return rootNode.Feed != null
                ? ODataResponse.FromFeed(rootNode.Feed, rootNode.TotalCount)
                : ODataResponse.FromEntry(rootNode.Entry);
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
            else
            {
                return value;
            }
        }
    }
}