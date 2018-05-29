using System;
using Xunit;

namespace Simple.OData.Client.Tests.Core
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