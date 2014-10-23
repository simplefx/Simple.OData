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

    class Event : PlanItem
    {
        public string Description { get; set; }
        public EventLocation OccursAt { get; set; }
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

    class EventLocation : Location
    {
        public string BuildingInfo { get; set; }
    }

    static class IODataClientExtensions
    {
        public static async Task<Airport> GetNearestAirportAsync(this IODataClient client, 
            double latitude, double longitude)
        {
            var result = await client.For<Airport>().ExecuteFunctionAsync("GetNearestAirport",
                    new Dictionary<string, object>()
                    {
                        {"lat", latitude}, 
                        {"lon", longitude},
                    });
            return result.First();
        }

        public static async Task ResetDataSource(this IODataClient client)
        {
            await client.ExecuteActionAsync<object>("ResetDataSource", null);
        }
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
        public async Task FindPersonPlanItems()
        {
            var flights = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo(x => x.Trips)
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .FindEntriesAsync();
            Assert.Equal(3, flights.Count());
        }

        [Fact]
        public async Task FindPersonFlight()
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
        public async Task FindPersonFlights()
        {
            var flights = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo(x => x.Trips)
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Flight>()
                .FindEntriesAsync();
            Assert.Equal(2, flights.Count());
            Assert.True(flights.Any(x => x.FlightNumber == "FM1930"));
        }

        [Fact]
        public async Task FindPersonFlightsWithFilter()
        {
            var flights = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo(x => x.Trips)
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Flight>()
                .Filter(x => x.FlightNumber == "FM1930")
                .FindEntriesAsync();
            Assert.Equal(1, flights.Count());
            Assert.True(flights.All(x => x.FlightNumber == "FM1930"));
        }

        [Fact]
        public async Task UpdatePersonFilterLastName()
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
        public async Task FindMeSelectAddressInfo()
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
        public async Task UpdateMeGender_PreconditionRequired()
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

        [Fact]
        public async Task InsertEvent()
        {
            var command = _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Event>();

            var tripEvent = await command
                .Set(new Event
                {
                    ConfirmationCode = "4372899DD",
                    Description = "Client Meeting",
                    Duration = TimeSpan.FromHours(3),
                    EndsAt = DateTimeOffset.Parse("2014-06-01T23:11:17.5479185-07:00"),
                    OccursAt = new EventLocation()
                    {
                        Address = "100 Church Street, 8th Floor, Manhattan, 10007",
                        BuildingInfo = "Regus Business Center",
                        City = new Location.LocationCity()
                        {
                            CountryRegion = "United States",
                            Name = "New York City",
                            Region = "New York",
                        }
                    },
                    PlanItemId = 33,
                    StartsAt = DateTimeOffset.Parse("2014-05-25T23:11:17.5459178-07:00"),
                })
                .InsertEntryAsync();

            tripEvent = await command
                .Key(tripEvent.PlanItemId)
                .FindEntryAsync();

            Assert.NotNull(tripEvent);
        }

        [Fact]
        public async Task UpdateEvent()
        {
            var command = _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Event>();

            var tripEvent = await command
                .Set(new Event
                {
                    ConfirmationCode = "4372899DD",
                    Description = "Client Meeting",
                    Duration = TimeSpan.FromHours(3),
                    EndsAt = DateTimeOffset.Parse("2014-06-01T23:11:17.5479185-07:00"),
                    OccursAt = new EventLocation()
                    {
                        Address = "100 Church Street, 8th Floor, Manhattan, 10007",
                        BuildingInfo = "Regus Business Center",
                        City = new Location.LocationCity()
                        {
                            CountryRegion = "United States",
                            Name = "New York City",
                            Region = "New York",
                        }
                    },
                    PlanItemId = 33,
                    StartsAt = DateTimeOffset.Parse("2014-05-25T23:11:17.5459178-07:00"),
                })
                .InsertEntryAsync();

            tripEvent = await command
                .Key(tripEvent.PlanItemId)
                .Set(new { Description = "This is a new description" })
                .UpdateEntryAsync();

            Assert.Equal("This is a new description", tripEvent.Description);
        }

        [Fact]
        public async Task DeleteEvent()
        {
            var command = _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Event>();

            var tripEvent = await command
                .Set(new Event
                {
                    ConfirmationCode = "4372899DD",
                    Description = "Client Meeting",
                    Duration = TimeSpan.FromHours(3),
                    EndsAt = DateTimeOffset.Parse("2014-06-01T23:11:17.5479185-07:00"),
                    OccursAt = new EventLocation()
                    {
                        Address = "100 Church Street, 8th Floor, Manhattan, 10007",
                        BuildingInfo = "Regus Business Center",
                        City = new Location.LocationCity()
                        {
                            CountryRegion = "United States",
                            Name = "New York City",
                            Region = "New York",
                        }
                    },
                    PlanItemId = 33,
                    StartsAt = DateTimeOffset.Parse("2014-05-25T23:11:17.5459178-07:00"),
                })
                .InsertEntryAsync();

            await command
                .Key(tripEvent.PlanItemId)
                .DeleteEntryAsync();

            tripEvent = await command
                .Key(tripEvent.PlanItemId)
                .FindEntryAsync();

            Assert.Null(tripEvent);
        }

        [Fact]
        public async Task FindPersonTrips()
        {
            var trips = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .FindEntriesAsync();

            Assert.Equal(3, trips.Count());
        }

        [Fact]
        public async Task FindPersonTripsFilterDescription()
        {
            var trips = await _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Filter(x => x.Description.Contains("New York"))
                .FindEntriesAsync();

            Assert.Equal(1, trips.Count());
            Assert.Contains("New York", trips.Single().Description);
        }

        [Fact]
        public async Task GetNearestAirport()
        {
            var airport = (await _client.GetNearestAirportAsync(100d, 100d));

            Assert.Equal("KSFO", airport.IcaoCode);
        }

        [Fact]
        public async Task ResetDataSource()
        {
            var command = _client
                .For<Person>("People")
                .Key("russellwhyte")
                .NavigateTo<Trip>()
                .Key(1003)
                .NavigateTo(x => x.PlanItems)
                .As<Event>();

            var tripEvent = await command
                .Set(new Event
                {
                    ConfirmationCode = "4372899DD",
                    Description = "Client Meeting",
                    Duration = TimeSpan.FromHours(3),
                    EndsAt = DateTimeOffset.Parse("2014-06-01T23:11:17.5479185-07:00"),
                    OccursAt = new EventLocation()
                    {
                        Address = "100 Church Street, 8th Floor, Manhattan, 10007",
                        BuildingInfo = "Regus Business Center",
                        City = new Location.LocationCity()
                        {
                            CountryRegion = "United States",
                            Name = "New York City",
                            Region = "New York",
                        }
                    },
                    PlanItemId = 33,
                    StartsAt = DateTimeOffset.Parse("2014-05-25T23:11:17.5459178-07:00"),
                })
                .InsertEntryAsync();

            await _client.ResetDataSource();

            tripEvent = await command
                .Filter(x => x.PlanItemId == tripEvent.PlanItemId)
                .FindEntryAsync();

            Assert.Null(tripEvent);
        }
    }
}