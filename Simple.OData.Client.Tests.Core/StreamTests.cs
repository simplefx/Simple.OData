using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class StreamTests : TestBase
    {
        class Photo
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public byte[] Media { get; set; }
        }

        [Fact]
        public async Task GetMediaStream()
        {
            var client = CreateClient("TripPin.xml");
            var command = client
                .For<Photo>()
                .Key(1)
                .Media();
            var commandText = await command.GetCommandTextAsync();
            Assert.Equal("Photos(1)/$value", commandText);
        }
    }
}