using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.OData;
using Microsoft.Data.Edm;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    internal class ResponseReaderV3 : IProviderResponseReader
    {
        class ResponseNode
        {
            public IList<IDictionary<string, object>> Feed { get; set; }
            public IDictionary<string, object> Entry { get; set; }
            public string LinkName { get; set; }
            public long? TotalCount { get; set; }

            public object Value
            {
                get
                {
                    if (this.Feed != null && this.Feed.Any())
                        return this.Feed.AsEnumerable();
                    else
                        return this.Entry;

                }
            }
        }

        private IODataResponseMessageAsync _response;
        private readonly IEdmModel _model;
        private readonly bool _includeResourceTypeInEntryProperties;

        public ResponseReaderV3(IODataResponseMessageAsync response, IEdmModel model, bool includeResourceTypeInEntryProperties = false)
        {
            _response = response;
            _model = model;
            _includeResourceTypeInEntryProperties = includeResourceTypeInEntryProperties;
        }

        public async Task<ODataResponse> GetResponseAsync()
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(_response, readerSettings, _model))
            {
                var payloadKind = messageReader.DetectPayloadKind();
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Value))
                {
                    var text = ProviderMetadata.StreamToString(await _response.GetStreamAsync());
                    return new ODataResponse(new[] { new Dictionary<string, object>() { { FluentCommand.ResultLiteral, text } } });
                }
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Property))
                {
                    var property = messageReader.ReadProperty();
                    return new ODataResponse(new[] { new Dictionary<string, object>() { { property.Name, property.Value } } });
                }
                var odataReader = payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Feed)
                    ? messageReader.CreateODataFeedReader()
                    : messageReader.CreateODataEntryReader();

                return ReadResponse(odataReader);
            }
        }

        //public async override Task<IEnumerable<IDictionary<string, object>>> GetEntriesAsync(HttpResponseMessage response)
        //{
        //    var result = await new ResponseReaderV3(new ODataV3ResponseMessage(response), Model, _settings.IncludeResourceTypeInEntryProperties).GetResponseAsync();
        //    return result.Entries;
        //}

        //public override async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> GetEntriesWithCountAsync(HttpResponseMessage response)
        //{
        //    var result = await new ResponseReaderV3(new ODataV3ResponseMessage(response), Model, _settings.IncludeResourceTypeInEntryProperties).GetResponseAsync();
        //    return Tuple.Create(result.Entries, (int)result.TotalCount.GetValueOrDefault());
        //}

        //public async override Task<IDictionary<string, object>> GetEntryAsync(HttpResponseMessage response)
        //{
        //    var result = await new ResponseReaderV3(new ODataV3ResponseMessage(response), Model, _settings.IncludeResourceTypeInEntryProperties).GetResponseAsync();
        //    return result.Entry;
        //}

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
                        foreach (var property in entry.Properties)
                        {
                            entryNode.Entry.Add(property.Name, GetPropertyValue(property.Value));
                        }
                        if (_includeResourceTypeInEntryProperties)
                        {
                            var resourceType = entry.TypeName;
                            entryNode.Entry.Add(FluentCommand.ResourceTypeLiteral, resourceType);
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
                            nodeStack.Peek().Entry.Add(linkNode.LinkName, linkNode.Value);
                        }
                        break;
                }
            }

            return rootNode.Feed != null 
                ? new ODataResponse(rootNode.Feed, rootNode.TotalCount) 
                : new ODataResponse(rootNode.Entry);
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