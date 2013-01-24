using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace Simple.Data.OData.Tests
{
    public class InheritanceTest : TestBase
    {
        [Fact]
        public void FindAllTransports()
        {
            IEnumerable<dynamic> transports = _db.Transports.All();

            Assert.NotEmpty(transports);
        }

        //[Fact]
        //public void InsertShip()
        //{
        //    var ship = _db.Ships.Insert(ShipName: "Test1");

        //    Assert.Equal("Test1", ship.ShipName);
        //}
    }
}