using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public class MediaClient : FluentClientBase<IDictionary<string, object>>, IMediaClient
    {
        internal MediaClient(ODataClient client, Session session, FluentCommand command = null, bool dynamicResults = false)
            : base(client, session, null, command, dynamicResults)
        {
        }

        public Task<Stream> GetStreamAsync()
        {
            return GetStreamAsync(CancellationToken.None);
        }

        public Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
        {
            return _client.GetMediaStreamAsync(_command, cancellationToken);
        }

        public Task<byte[]> GetStreamAsArrayAsync()
        {
            return GetStreamAsArrayAsync(CancellationToken.None);
        }

        public async Task<byte[]> GetStreamAsArrayAsync(CancellationToken cancellationToken)
        {
            using (var stream = await _client.GetMediaStreamAsync(_command, cancellationToken))
            {
                return Utils.StreamToByteArray(stream);
            }
        }

        public Task<string> GetStreamAsStringAsync()
        {
            return GetStreamAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetStreamAsStringAsync(CancellationToken cancellationToken)
        {
            using (var stream = await _client.GetMediaStreamAsync(_command, cancellationToken))
            {
                return Utils.StreamToString(stream);
            }
        }

        public Task SetStreamAsync(Stream stream)
        {
            return SetStreamAsync(stream, CancellationToken.None);
        }

        public Task SetStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            return _client.SetMediaStreamAsync(_command, stream, cancellationToken);
        }

        public Task SetStreamAsync(byte[] streamContent)
        {
            return SetStreamAsync(streamContent, CancellationToken.None);
        }

        public Task SetStreamAsync(byte[] streamContent, CancellationToken cancellationToken)
        {
            return _client.SetMediaStreamAsync(_command, Utils.ByteArrayToStream(streamContent), cancellationToken);
        }

        public Task SetStreamAsync(string streamContent)
        {
            return SetStreamAsync(streamContent, CancellationToken.None);
        }

        public Task SetStreamAsync(string streamContent, CancellationToken cancellationToken)
        {
            return _client.SetMediaStreamAsync(_command, Utils.StringToStream(streamContent), cancellationToken);
        }
    }
}
