using System.Linq;
using System.Threading.Tasks;
using Microsoft.Spatial;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindTripPinTestsV4Json : FindTripPinTests
    {
        public FindTripPinTestsV4Json() : base(TripPinV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    class Airline
    {
        public string AirlineCode { get; set; }
        public string Name { get; set; }
    }

    class Airport
    {
        public string IcaoCode { get; set; }
        public string IataCode { get; set; }
        public string Name { get; set; }
        public AirportLocation Location { get; set; }
    }

    class Location
    {
        public class LocationCity
        {
            public string CountryRegion { get; set; }
            public string Name { get; set; }
            public string Region { get; set; }
        }

        public string Address { get; set; }
        public LocationCity City { get; set; }
    }

    class AirportLocation : Location
    {
        public GeographyPoint Loc { get; set; }
    }

    public abstract class FindTripPinTests : TripPinTestBase
    {
        protected FindTripPinTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) {}

        [Fact]
        public async Task AllAirlines()
        {
            var airlines = await _client
                .For<Airline>()
                .FindEntriesAsync();
            Assert.Equal(8, airlines.Count());
        }

        [Fact]
        public async Task AllAirports()
        {
            var airports = await _client
                .For<Airport>()
                .FindEntriesAsync();
            Assert.Equal(8, airports.Count());
        }

        [Fact]
        public async Task AirportByCode()
        {
            var airport = await _client
                .For<Airport>()
                .Key("KSFO")
                .FindEntryAsync();
            Assert.Equal("SFO", airport.IataCode);
            Assert.Equal("San Francisco", airport.Location.City.Name);
            Assert.Equal(4326, airport.Location.Loc.CoordinateSystem.EpsgId);
            Assert.Equal(37.6188888888889, airport.Location.Loc.Latitude);
            Assert.Equal(-122.374722222222, airport.Location.Loc.Longitude);
        }
    }
}