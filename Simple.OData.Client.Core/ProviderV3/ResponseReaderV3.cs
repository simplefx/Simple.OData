using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.OData;
using Microsoft.Data.Edm;

namespace Simple.OData.Client
{
    internal class ResponseReaderV3
    {
        private readonly IODataResponseMessageAsync _response;
        private readonly IEdmModel _model;

        public ResponseReaderV3(IODataResponseMessageAsync response, IEdmModel model)
        {
            _response = response;
            _model = model;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> GetEntriesAsync()
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(_response, readerSettings, _model))
            {
                var entries = new List<IDictionary<string, object>>();
                var payloadKind = messageReader.DetectPayloadKind();
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Value))
                {
                    var text = ProviderMetadata.StreamToString(await _response.GetStreamAsync());
                    return new[] { new Dictionary<string, object>() { { FluentCommand.ResultLiteral, text } } };
                }
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Property))
                {
                    var property = messageReader.ReadProperty();
                    return new[] { new Dictionary<string, object>() { { property.Name, property.Value } } };
                }
                var odataReader = payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Feed)
                    ? messageReader.CreateODataFeedReader()
                    : messageReader.CreateODataEntryReader();
                entries.AddRange(ReadData(odataReader));
                return entries;
            }
        }

        public async Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> GetEntriesWithCountAsync()
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(_response, readerSettings, _model))
            {
                var entries = new List<IDictionary<string, object>>();
                var odataReader = messageReader.CreateODataFeedReader();
                long totalCount;
                entries.AddRange(ReadData(odataReader, out totalCount));
                return Tuple.Create(entries.AsEnumerable(), (int)totalCount);
            }
        }

        public async Task<IDictionary<string, object>> GetEntryAsync()
        {
            var readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas.MaxReceivedMessageSize = Int32.MaxValue;
            using (var messageReader = new ODataMessageReader(_response, readerSettings, _model))
            {
                var payloadKind = messageReader.DetectPayloadKind();
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Value))
                {
                    var text = ProviderMetadata.StreamToString(await _response.GetStreamAsync());
                    return new Dictionary<string, object>() { { FluentCommand.ResultLiteral, text } };
                }
                if (payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Property))
                {
                    var property = messageReader.ReadProperty();
                    return new Dictionary<string, object>() { { property.Name, property.Value } };
                }
                var odataReader = payloadKind.Any(x => x.PayloadKind == ODataPayloadKind.Feed)
                    ? messageReader.CreateODataFeedReader()
                    : messageReader.CreateODataEntryReader();
                return ReadData(odataReader).FirstOrDefault();
            }
        }

        private IEnumerable<IDictionary<string, object>> ReadData(ODataReader odataReader)
        {
            while (odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataReaderState.FeedStart:
                        return ReadEntries(odataReader, false);
                    case ODataReaderState.EntryStart:
                        return new[] { ReadEntry(odataReader, false) };
                }
            }
            return null;
        }

        private IEnumerable<IDictionary<string, object>> ReadData(ODataReader odataReader, out long totalCount)
        {
            totalCount = 0;
            while (odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataReaderState.FeedStart:
                        totalCount = (odataReader.Item as ODataFeed).Count.GetValueOrDefault();
                        return ReadEntries(odataReader, false);
                    case ODataReaderState.EntryStart:
                        return new[] { ReadEntry(odataReader, false) };
                }
            }
            return null;
        }

        private IEnumerable<IDictionary<string, object>> ReadEntries(ODataReader odataReader, bool isNavigation)
        {
            if (odataReader.State == ODataReaderState.Completed)
                return null;

            var entries = new List<IDictionary<string, object>>();
            while (odataReader.State != ODataReaderState.Completed && odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataReaderState.FeedEnd:
                    case ODataReaderState.NavigationLinkEnd:
                    case ODataReaderState.Completed:
                        return entries;

                    case ODataReaderState.EntryStart:
                        entries.Add(ReadEntry(odataReader, false));
                        break;
                }
            }
            return entries;
        }

        private IDictionary<string, object> ReadEntry(ODataReader odataReader, bool isNavigation)
        {
            if (odataReader.State == ODataReaderState.Completed)
                return null;

            var entry = new Dictionary<string, object>();
            while (odataReader.State != ODataReaderState.Completed && odataReader.Read())
            {
                switch (odataReader.State)
                {
                    case ODataReaderState.EntryEnd:
                        foreach (var property in (odataReader.Item as Microsoft.Data.OData.ODataEntry).Properties)
                        {
                            entry.Add(property.Name, property.Value);
                        }
                        return entry;

                    case ODataReaderState.NavigationLinkEnd:
                        return entry.Any() ? entry : null;

                    case ODataReaderState.NavigationLinkStart:
                        var link = odataReader.Item as ODataNavigationLink;
                        if (link.IsCollection.HasValue && link.IsCollection.Value)
                        {
                            entry.Add(link.Name, ReadEntries(odataReader, true));
                        }
                        else
                        {
                            entry.Add(link.Name, ReadEntry(odataReader, true));
                        }
                        break;

                    case ODataReaderState.Completed:
                        return entry;
                }
            }
            return entry;
        }
    }
}