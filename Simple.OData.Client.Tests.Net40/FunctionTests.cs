using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class FunctionTests : TestBase
    {
        [Fact]
        public async Task FunctionWithString()
        {
            var result = await _client.ExecuteFunctionAsScalarAsync<int>("ParseInt", new Entry() { { "number", "1" } });
            
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task FunctionWithIntCollectionSingleElement()
        {
            var result = await _client.ExecuteFunctionAsArrayAsync<int>("ReturnIntCollection", 
                new Entry() { { "count", 1 } });
            
            Assert.Equal(new[] { 1 }, result);
        }

        [Fact]
        public async Task FunctionWithIntCollectionMultipleElements()
        {
            var result = await _client.ExecuteFunctionAsArrayAsync<int>("ReturnIntCollection", 
                new Entry() { { "count", 3 } });
            
            Assert.Equal(new[] { 1, 2, 3 }, result);
        }

        [Fact]
        public async Task FunctionWithLong()
        {
            var result = await _client.ExecuteFunctionAsScalarAsync<long>("PassThroughLong", new Entry() { { "number", 1L } });
            
            Assert.Equal(1L, result);
        }

        [Fact]
        public async Task FunctionWithDateTime()
        {
            var dateTime = new DateTime(2013, 1, 1, 12, 13, 14);
            var result = await _client.ExecuteFunctionAsScalarAsync<DateTime>("PassThroughDateTime", new Entry() { { "dateTime", dateTime } });
            
            Assert.Equal(dateTime.ToLocalTime(), result);
        }

        [Fact]
        public async Task FunctionWithGuid()
        {
            var guid = Guid.NewGuid();
            var result = await _client.ExecuteFunctionAsScalarAsync<Guid>("PassThroughGuid", new Entry() { { "guid", guid } });
            
            Assert.Equal(guid, result);
        }

        [Fact]
        public async Task FunctionWithComplexTypeCollectionSingleElement()
        {
            var result = await _client.ExecuteFunctionAsArrayAsync<IDictionary<string, object>>("ReturnAddressCollection", 
                new Entry() { { "count", 1 } });

            Assert.Equal("Oslo", result[0]["City"]);
            Assert.Equal("Norway", result[0]["Country"]);
        }

        [Fact]
        public async Task FunctionWithComplexTypeCollectionMultipleElements()
        {
            var result = await _client.ExecuteFunctionAsArrayAsync<IDictionary<string, object>>("ReturnAddressCollection", 
                new Entry() { { "count", 3 } });

            Assert.Equal("Oslo", result[0]["City"]);
            Assert.Equal("Norway", result[0]["Country"]);
            Assert.Equal("Oslo", result[1]["City"]);
            Assert.Equal("Oslo", result[2]["City"]);
        }
    }
}
