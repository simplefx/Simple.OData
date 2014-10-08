using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Spatial;
using Xunit;

namespace Simple.OData.Client.Tests
{
#if ODATA_V4
    public class TripPinTestsV4Json : TripPinTests
    {
        public TripPinTestsV4Json() : base(TripPinV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }
#endif

    enum PersonGender
    {
        Male,
        Female,
        Unknown,
    }

    class Person
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Emails { get; set; }
        public List<Location> AddressInfo { get; set; }
        public PersonGender Gender { get; set; }
        public long Concurrency { get; set; }

        public IEnumerable<Person> Friends { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<Photo> Photos { get; set; }
    }

    class Trip
    {
        public int TripId { get; set; }
        public Guid ShareId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public float Budget { get; set; }
        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public IList<string> Tags { get; set; }

        public IEnumerable<Photo> Photos { get; set; }
        public IEnumerable<PlanItem> PlanItems { get; set; }
    }

    class Photo
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    class PlanItem
    {
        public int PlanItemId { get; set; }
        public string ConfirmationCode { get; set; }
        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public TimeSpan Duration { get; set; }
    }

    class PublicTransportation : PlanItem
    {
        public string SeatNumber { get; set; }
    }

    class Flight : PublicTransportation
    {
        public string FlightNumber { get; set; }
        public Airport From { get; set; }
        public Airport To { get; set; }
        public Airline Airline { get; set; }
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

    public abstract class TripPinTests : TripPinTestBase
    {
        protected TripPinTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task FindAllPeople()
        {
            var people = await _client
                .For<Person>("People")
                .FindEntriesAsync();
            Assert.Equal(8, people.Count());
        }

        [Fact]
        public async Task FindPerson_Flight()
        {
            var flight = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo(x => x.Trips)
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .Key(21)
                .As<Flight>()
                .FindEntryAsync();
            Assert.Equal("FM1930", flight.FlightNumber);
        }

        [Fact]
        public async Task UpdatePerson_LastName()
        {
            var person = await _client
                .For<Person>("People")
                .Filter(x => x.UserName == "russellwhyte")
                .Set(new { LastName = "White" })
                .UpdateEntryAsync();
            Assert.Equal("White", person.LastName);
        }

        [Fact]
        public async Task FindMe()
        {
            var person = await _client
                .For<Person>("Me")
                .FindEntryAsync();
            Assert.Equal("aprilcline", person.UserName);
            Assert.Equal(2, person.Emails.Count());
            Assert.Equal("Lander", person.AddressInfo.Single().City.Name);
            Assert.Equal(PersonGender.Female, person.Gender);
        }

        [Fact]
        public async Task FindMe_SelectAddressInfo()
        {
            var person = await _client
                .For<Person>("Me")
                .Select(x => x.AddressInfo)
                .FindEntryAsync();
            Assert.Equal("Lander", person.AddressInfo.Single().City.Name);
            Assert.Null(person.UserName);
            Assert.Null(person.Emails);
        }

        [Fact]
        public async Task UpdateMe_Gender_PreconditionRequired()
        {
            AssertThrowsAsync<AggregateException>(async () =>
            {
                await _client
                    .For<Person>("Me")
                    .Set(new { Gender = PersonGender.Male })
                    .UpdateEntryAsync();
            });
        }

        //[Fact]
        //public async Task UpdateMe_LastName_PreconditionRequired()
        //{
        //    var person = await _client
        //        .For<Person>("Me")
        //        .Set(new { LastName = "newname" })
        //        .UpdateEntryAsync();
        //    Assert.Equal("newname", person.LastName);
        //}

        [Fact]
        public async Task FindAllAirlines()
        {
            var airlines = await _client
                .For<Airline>()
                .FindEntriesAsync();
            Assert.Equal(8, airlines.Count());
        }

        [Fact]
        public async Task FindAllAirports()
        {
            var airports = await _client
                .For<Airport>()
                .FindEntriesAsync();
            Assert.Equal(8, airports.Count());
        }

        [Fact]
        public async Task FindAirportByCode()
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