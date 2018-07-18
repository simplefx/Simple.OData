using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Simple.OData.Client.Tests;
using Xunit;

namespace Simple.OData.Client.Benchmarks
{
    class PeopleCollection
    {
        [JsonProperty("value")]
        public Person [] People { get; set; }
    }

    public class TripPinPeople
    {
        [Benchmark]
        public void FindTypedPeopleWithTripsAndFriends()
        {
            var result = Utils.GetClient("TripPin.xml", "TripPin_result_20.json")
                .For<Person>()
                .Expand(x => new {x.Trips, x.Friends})
                .FindEntriesAsync()
                .Result.ToList();
            Assert.Equal(20, result.Count);
        }

        [Benchmark]
        public void FindUntypedPeopleWithTripsAndFriends()
        {
            var result = Utils.GetClient("TripPin.xml", "TripPin_result_20.json")
                .For("People")
                .Expand(new [] {"Trips", "Friends"})
                .FindEntriesAsync()
                .Result.ToList();
            Assert.Equal(20, result.Count);
        }

        [Benchmark]
        public void ConvertWithNewtonsoftJson()
        {
            var json = File.ReadAllText(@"..\..\..\..\..\..\..\Resources\" + "TripPin_result_20.json");
            var result = JsonConvert.DeserializeObject<PeopleCollection>(json);
            Assert.Equal(20, result.People.Length);
        }
    }
}
