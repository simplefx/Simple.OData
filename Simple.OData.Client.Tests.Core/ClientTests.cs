using System;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class ClientTests
    {
        [Fact]
        public void SettingsWithNoUri()
        {
            Assert.Throws<InvalidOperationException>(() => new ODataClient(new ODataClientSettings()));
        }
    }
}