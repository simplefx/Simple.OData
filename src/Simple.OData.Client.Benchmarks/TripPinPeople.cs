using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Simple.OData.Client;
using Simple.OData.Client.Tests;
using Xunit;

namespace Simple.OData.Client.Benchmarks
{
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
    }
}
