using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class StreamTests : TestBase
    {
        public override string MetadataFile { get { return "TripPin.xml"; } }
        public override IFormatSettings FormatSettings { get { return new ODataV4Format(); } }

        class Photo
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public byte[] Media { get; set; }
        }

        [Fact]
        public async Task GetMediaStream()
        {
            var command = _client
                .For<Photo>()
                .Key(1)
                .Media();
            var commandText = await command.GetCommandTextAsync();
            Assert.Equal("Photos(1)/$value", commandText);
        }
    }
}